using Discord.Audio;
using MusicSearch;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mirai.Audio
{
    class Streamer
    {
        private static string FilterText = string.Empty;
        internal static string Filter
        {
            get
            {
                return FilterText == string.Empty ? FilterText : $"-af \"{FilterText}\"";
            }
            set
            {
                FilterText = value;
            }
        }

        internal static SongQueue Queue = new SongQueue();
        private static CancellationTokenSource Cancel;
        internal static CancellationTokenSource Skip;

        internal static TimeSpan Duration;
        internal static TimeSpan Time;
        internal static long RemainingTicks;

        internal static async Task Restart()
        {
            Cancel?.Cancel();
            Skip?.Cancel();
            Cancel = new CancellationTokenSource();

            const int Stride = 2880 * 2 * 2;
            var Buffer = new byte[2 * Stride];
            int Swapper = 0;

            AudioOutStream Out;
            while (!Cancel.IsCancellationRequested)
            {
                Duration = default(TimeSpan);
                Time = default(TimeSpan);
                RemainingTicks = long.MaxValue;

                if ((Out = await Connection.GetStream()) != null && Queue.Next())
                {
                    Bot.Channel().SendMessageAsync($"Now playing {Queue.Playing.Title}", true);
                    Skip = new CancellationTokenSource();

                    var FFMpeg = Process.Start(new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-re -i pipe:0 {Filter} -f s16le -ar 48k -ac 2 pipe:1",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    });
                    
                    FFMpeg.ErrorDataReceived += async (s, e) =>
                    {
                        var Text = e?.Data?.Trim();
                        if (Text != null)
                        {
                            Logger.Log(Text);

                            if (Text.StartsWith("Duration: "))
                            {
                                TimeSpan.TryParse(Text.Substring(10).Split(new[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries)[0], out Duration);
                            }
                            else if (Text.StartsWith("size="))
                            {
                                TimeSpan.TryParse(Text.Split(new[] { "time=" }, 2, StringSplitOptions.RemoveEmptyEntries)[1].Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries)[0], out Time);
                            }

                            if (Duration != default(TimeSpan) && Time != default(TimeSpan) && (RemainingTicks = Duration.Ticks - Time.Ticks) <= 0)
                            {
                                Skip?.Cancel();
                            }
                        }
                    };

                    FFMpeg.BeginErrorReadLine();

                    var Url = await Queue.StreamUrl();
                    BufferAsync(Url.StartsWith(SongRequestLocal.MusicDir) ?
                            File.OpenRead(Url) :
                            await new HttpClient(new HttpClientHandler()).GetStreamAsync(Url),
                            FFMpeg.StandardInput.BaseStream, Skip.Token);

                    using (var Pipe1 = FFMpeg.StandardOutput.BaseStream)
                    {
                        int Read = await Pipe1.ReadAsync(Buffer, Swapper * Stride, Stride, Skip.Token);
                        while (Read != 0 && !Skip.IsCancellationRequested)
                        {
                            var Send = Out.WriteAsync(Buffer, Swapper * Stride, Read, Skip.Token);

                            Swapper = 1 - Swapper;
                            Read = await Pipe1.ReadAsync(Buffer, Swapper * Stride, Stride, Skip.Token);

                            await Send;
                        }
                    }

                    Logger.Log("Disposed FFMpeg Pipe 1");
                    Skip = null;

                    try
                    {
                        FFMpeg.Kill();
                    }
                    catch { }
                }
                else
                {
                    await Task.Delay(50);
                }
            }
        }

        private static async Task BufferAsync(Stream In, Stream Pipe0, CancellationToken Token)
        {
            try
            {
                using (Pipe0)
                {
                    var AsyncSender = Task.Delay(0);

                    var Buff = new byte[32 * 1024];
                    int Read;
                    while (!Token.IsCancellationRequested && (Read = await In.ReadAsync(Buff, 0, Buff.Length, Skip.Token)) != 0)
                    {
                        if (RemainingTicks > 30 * 10000000)
                        {
                            await Pipe0.WriteAsync(Buff, 0, Read, Skip.Token);
                        }
                        else
                        {
                            var NewBuff = new byte[Read];
                            Buffer.BlockCopy(Buff, 0, NewBuff, 0, Read);

                            AsyncSender = AsyncSender.ContinueWith(async t =>
                            {
                                await Pipe0.WriteAsync(NewBuff, 0, NewBuff.Length, Skip.Token);
                            });
                        }
                    }

                    await AsyncSender;
                }
            }
            catch (Exception Ex)
            {
                Logger.Log(Ex);
            }
            finally
            {
                Logger.Log("Disposed FFMpeg Pipe 0");
            }
        }
    }
}
