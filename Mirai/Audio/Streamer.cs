using Discord.Audio;
using MusicSearch;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mirai.Audio
{
    class Streamer
    {
        internal static SongQueue Queue = new SongQueue();
        private static CancellationTokenSource Cancel;
        internal static CancellationTokenSource Skip;

        internal static async Task Restart()
        {
            Cancel?.Cancel();
            Cancel = new CancellationTokenSource();

            const int Stride = 4096;
            var Buffer = new byte[2 * Stride];
            int Swapper = 0;

            AudioOutStream Out;
            while (!Cancel.IsCancellationRequested)
            {
                if ((Out = await Connection.GetStream()) != null && Queue.Next())
                {
                    Skip = new CancellationTokenSource();

                    var FFMpeg = Process.Start(new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-re -i \"{await Queue.StreamUrl()}\" -f s16le -ar 48k pipe:1",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    });
                    
                    FFMpeg.BeginErrorReadLine();
                    FFMpeg.ErrorDataReceived += async (s, e) =>
                    {
                        if (e?.Data?.Contains("Press [q] to stop, [?] for help") ?? false)
                        {
                            FFMpeg.CancelErrorRead();
                            Bot.Channel().SendMessageAsync($"Playing {Queue.Playing.Title}", true);
                        }
                    };

                    var In = FFMpeg.StandardOutput.BaseStream;

                    int Read = await In.ReadAsync(Buffer, Swapper * Stride, Stride);
                    while (Read != 0 && !Skip.IsCancellationRequested && !Cancel.IsCancellationRequested)
                    {
                        var Send = Out.WriteAsync(Buffer, Swapper * Stride, Read);

                        Swapper = 1 - Swapper;
                        Read = await In.ReadAsync(Buffer, Swapper * Stride, Stride);

                        await Send;
                    }

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
    }
}
