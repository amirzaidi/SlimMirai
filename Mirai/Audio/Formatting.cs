using Discord;
using Discord.Rest;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mirai.Audio
{
    class Formatting
    {
        private static RestUserMessage Message;
        private static CancellationTokenSource Cancel;

        internal static async Task Update(string TTSMessage)
        {
            Cancel?.Cancel();
            Cancel = new CancellationTokenSource();
            
            try
            {
                var New = await Bot.SendTTS(TTSMessage, new EmbedBuilder()
                    .WithTitle(Streamer.Queue.IsPlaying ? $"♫ {Streamer.Queue.Playing.Title} ♫" : "Nothing is playing")
                    .WithUrl(Streamer.Queue.IsPlaying && Streamer.Queue.Playing.Url.StartsWith("http") ? Streamer.Queue.Playing.Url : "https://github.com/amirzaidi/slimmirai")
                    .WithThumbnailUrl(Streamer.Queue.Playing.ThumbNail)
                    .WithColor(new Color(0xFF5722))
                    .WithDescription(string.Join("\n", Streamer.Queue.Titles))
                    .Build(), new RequestOptions
                    {
                        CancelToken = Cancel.Token
                    }
                );

                if (Message != null)
                {
                    await Message.DeleteAsync();
                }

                Message = New;
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception Ex)
            {
                Logger.Log(Ex);
            }
        }
    }
}
