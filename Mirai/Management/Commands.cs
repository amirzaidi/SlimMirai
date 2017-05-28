using Discord.WebSocket;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Mirai.Management
{
    class Commands
    {
        internal static async Task Shutdown(ulong User, Queue<string> Args)
        {
            //Save State
            Program.Shutdown.TrySetResult(true);
        }

        internal static async Task Voice(string s, SocketMessage e)
        {
            var Values = new Queue<string>(s.Split(' '));
            var Rank = User.GetRank(e.Author.Id);
            var Cmd = Command.GetVoice(string.Join(" ", Values), Rank);
            if (Cmd == null)
            {
                Command.GetVoice(Values.Dequeue(), Rank)?.Invoke(e.Author.Id, Values);
            }
            else
            {
                Values.Clear();
                Cmd.Invoke(e.Author.Id, Values);
            }
        }

        internal static async Task Name(string s, SocketMessage e)
        {
            await Bot.User.ModifyAsync(x => x.Username = s);
        }

        internal static async Task Avatar(string s, SocketMessage e)
        {
            foreach (var Attachment in e.Attachments)
                if (Attachment.Height != null)
                {
                    var Request = (HttpWebRequest)WebRequest.Create(Attachment.Url);

                    using (var Response = await Request.GetResponseAsync())
                    using (var Stream = new MemoryStream())
                    {
                        using (var Picture = Image.FromStream(Response.GetResponseStream()))
                            Picture.Save(Stream, ImageFormat.Bmp);

                        Stream.Seek(0, SeekOrigin.Begin);
                        await Bot.User.ModifyAsync(x => x.Avatar = new Discord.Image(Stream));
                    }
                }
        }
    }
}
