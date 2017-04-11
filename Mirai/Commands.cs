using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MusicSearch;

namespace Mirai.Audio
{
    static class Commands
    {
        public static async Task Add(string s, SocketMessage e)
        {
            Formatting.Channel = e.Channel;

            Logger.Log("Adding music: " + s);
            var Music = await SongRequest.Search(s);
            if (Music.Count != 0)
            {
                Streamer.Queue.Enqueue(Music[0]);
                Formatting.Update();
            }
        }

        public static async Task Join(string s, SocketMessage e)
        {
            Connection.JoinSame(e.Author as IGuildUser);
        }
    }
}
