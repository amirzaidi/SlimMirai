using Discord.Audio;
using MusicSearch;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Mirai.Audio
{
    class Streamer
    {
        internal static SongQueue Queue = new SongQueue();
        private static CancellationTokenSource Cancel;
        private static CancellationTokenSource Skip;
        private static double SS = 0;

        internal static TimeSpan Duration;
        internal static TimeSpan Time;
        internal static long TicksRemaining;

        internal const int Samples = 2880;

        const int Stride = Samples * 2;
        static byte[] BuffOut = new byte[2 * Stride];
        static int Swapper = 0;

        internal static string PlaybackSpeed = string.Empty;

        internal static void Stop()
        {
            Cancel?.Cancel();
            Skip?.Cancel();
        }

        internal static async Task Start(IAudioClient Client)
        {
            Cancel = new CancellationTokenSource();

            while (!Cancel.IsCancellationRequested)
            {
                Duration = default(TimeSpan);
                Time = default(TimeSpan);
                TicksRemaining = long.MaxValue;

                try
                {
                    await Queue.Next(Cancel.Token);

                    if (SS == 0)
                    {
                        Formatting.Update($"Now playing {Queue.Playing.Title}");
                        Bot.Client.SetGameAsync(Queue.Playing.Title);
                    }

                    Skip = new CancellationTokenSource();

                    await Client.SetSpeakingAsync(true);
                    using (var Out = Client.CreateDirectPCMStream(AudioApplication.Music, 128 * 1024, Filter.Packets))
                        await StreamAsync(Out);

                    Queue.ResetPlaying();
                }
                catch (Exception Ex)
                {
                    Logger.Log(Ex);
                }
            }
        }

        internal static void Next()
        {
            SS = 0;
            Skip?.Cancel();
        }

        internal static void ReloadSong()
        {
            if (Queue.IsPlaying)
            {
                SS += Time.TotalSeconds + 0.4;
                Queue.Repeat(1);
                Skip?.Cancel();
            }
        }

        private static async Task StreamAsync(AudioOutStream Out)
        {
            var FFMpeg = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-ss {SS.ToString(System.Globalization.CultureInfo.InvariantCulture)} -re -i pipe:0 -f s16le -ar 48k -ac 2 -af \"{Filter.Tag}\" pipe:1",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            FFMpeg.ErrorDataReceived += async (s, e) =>
            {
                var FFLog = e?.Data?.Trim();
                if (FFLog != null)
                {
                    if (FFLog.StartsWith("Duration: "))
                    {
                        TimeSpan.TryParse(FFLog.Substring(10).Split(new[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries)[0], out Duration);
                        Logger.Log("Enabled buffer acceleration during the last 3 minutes");
                    }
                    else if (FFLog.StartsWith("size="))
                    {
                        var SplitTime = FFLog.Split(new[] { "time=" }, 2, StringSplitOptions.RemoveEmptyEntries)[1];
                        TimeSpan.TryParse(SplitTime.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries)[0], out Time);
                        var SpeedSplit = SplitTime.Split('=');
                        PlaybackSpeed = SpeedSplit[SpeedSplit.Length - 1].Trim();
                    }

                    if (Duration != default(TimeSpan) && Time != default(TimeSpan) && (TicksRemaining = Duration.Ticks - Time.Ticks) <= 0)
                        Skip?.Cancel();
                }
            };

            FFMpeg.BeginErrorReadLine();
            BufferAsync(await GetStream(await Queue.StreamUrl()), FFMpeg.StandardInput.BaseStream, Skip.Token);

            using (var Pipe1 = FFMpeg.StandardOutput.BaseStream)
                try
                {
                    int Read = await Pipe1.ReadAsync(BuffOut, Swapper * Stride, Stride, Skip.Token);
                    while (Read != 0)
                    {
                        var Send = Out.WriteAsync(BuffOut, Swapper * Stride, Read, Skip.Token);

                        Swapper = Swapper ^ 1;
                        Read = await Pipe1.ReadAsync(BuffOut, Swapper * Stride, Stride, Skip.Token);

                        await Send;
                    }

                    SS = 0; //After full process without skip
                }
                catch (TaskCanceledException)
                {
                }
                catch (OperationCanceledException)
                {
                }

            Logger.Log("Disposed FFMpeg Pipe 1");

            try
            {
                FFMpeg.Kill();
            }
            catch
            {
            }
        }

        private static async Task<Stream> GetStream(string Url)
        {
            try
            {
                return File.OpenRead(Url);
            }
            catch (ArgumentException)
            {
            }
            catch (PathTooLongException)
            {
            }
            catch (Exception Ex)
            {
                Logger.Log(Ex);
            }

            if (Uri.TryCreate(Url, UriKind.Absolute, out Uri Result))
            {
                var Response = (HttpWebResponse)await WebRequest.Create(Result).GetResponseAsync();
                return Response.GetResponseStream();
            }

            return Stream.Null;
        }

        private static async Task BufferAsync(Stream In, Stream Pipe0, CancellationToken Token)
        {
            try
            {
                var SendChain = Task.CompletedTask;

                var Buff = new byte[32 * 1024];
                int Read;

                using (Pipe0)
                {
                    using (In)
                        while (!Token.IsCancellationRequested && (Read = await In.ReadAsync(Buff, 0, Buff.Length, Skip.Token)) != 0)
                            if (TicksRemaining > 210 * 10000000)
                                await Pipe0.WriteAsync(Buff, 0, Read, Skip.Token);
                            else
                            {
                                var Clone = new byte[Read];
                                Buffer.BlockCopy(Buff, 0, Clone, 0, Read);

                                SendChain = SendChain.ContinueWith(async t => await Pipe0.WriteAsync(Clone, 0, Clone.Length, Skip.Token));
                            }

                    await SendChain;
                }
            }
            catch (IOException)
            {
            }
            catch (Exception Ex)
            {
                Logger.Log(Ex);
            }

            Logger.Log("Disposed FFMpeg Pipe 0");
        }
    }
}
