using System;
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
            Logger.Log("Adding music: " + s);
            var Music = await SongRequest.Search(s);
            if (Music.Count != 0)
            {
                await e.Channel.SendMessageAsync("Added music: " + Music[0].Title);
                Streamer.Queue.Enqueue(Music[0]);
            }
        }

        public static async Task Join(string s, SocketMessage e)
        {
            Connection.JoinSame(e.Author as IGuildUser);
        }
    }
}
