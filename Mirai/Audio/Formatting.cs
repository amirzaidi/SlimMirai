using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mirai.Audio
{
    class Formatting
    {
        private static RestUserMessage Message;
        private static CancellationTokenSource Cancel;

        internal static async Task Update()
        {
            Cancel?.Cancel();
            Cancel = new CancellationTokenSource();
            
            try
            {
                var New = await Bot.Channel().SendMessageAsync(string.Empty, embed: new EmbedBuilder()
                    .WithTitle(Streamer.Queue.IsPlaying ? $"♫ {Streamer.Queue.Playing.Title} ♫" : "Nothing is playing")
                    .WithThumbnailUrl(Streamer.Queue.Playing.ThumbNail)
                    .WithColor(new Color(0xFF5722))
                    .WithDescription(string.Join("\n", Streamer.Queue.Titles))
                    .Build(), options: new RequestOptions()
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
            catch (Exception Ex)
            {
                Logger.Log(Ex);
            }
        }
    }
}
