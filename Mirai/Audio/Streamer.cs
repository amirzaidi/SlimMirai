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

        internal static void Stop()
        {
            Cancel?.Cancel();
        }

        internal static async Task Restart()
        {
            Stop();
            Cancel = new CancellationTokenSource();

            const int Stride = 4096;
            var Buffer = new byte[2 * Stride];
            int Swapper = 0;

            AudioOutStream Out;

            while (!Cancel.IsCancellationRequested)
            {
                if ((Out = await Connection.GetStream()) != null && Queue.Next())
                {
                    var FFMpeg = Process.Start(new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-re -i async:\"{await Queue.StreamUrl()}\" -f s16le -ar 48k -v 0 pipe:1",
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    });

                    var In = FFMpeg.StandardOutput.BaseStream;

                    int Read = await In.ReadAsync(Buffer, Swapper * Stride, Stride);
                    while (Read != 0)
                    {
                        var Send = Out.WriteAsync(Buffer, Swapper * Stride, Read);

                        Swapper = 1 - Swapper;
                        Read = await In.ReadAsync(Buffer, Swapper * Stride, Stride);

                        await Send;
                    }
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }
    }
}
