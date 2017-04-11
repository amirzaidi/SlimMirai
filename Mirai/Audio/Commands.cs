using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MusicSearch;

namespace Mirai.Audio
{
    static class Commands
    {
        internal static async Task Add(string s, SocketMessage e)
        {
            Logger.Log("Adding music: " + s);
            var Music = await SongRequest.Search(s);
            if (Music.Count != 0)
            {
                Streamer.Queue.Enqueue(Music[0]);
                Formatting.Update();
            }
        }

        internal static async Task Join(string s, SocketMessage e)
        {
            Connection.JoinSame(e.Author as IGuildUser);
        }

        internal static async Task Skip(string Text, ulong User)
        {
            Logger.Log($"{User} {Text}");
            Streamer.Skip?.Cancel();
            Formatting.Update();
        }

        internal static async Task Remove(string Text, ulong User)
        {
            Logger.Log($"{User} {Text}");
        }

        internal static async Task Move(string Text, ulong User)
        {
            Logger.Log($"{User} {Text}");
        }

        internal static async Task Local(string Text, ulong User)
        {
            Logger.Log("Adding local music: " + Text);
            var Music = await SongRequest.Search(Text);
            if (Music.Count != 0)
            {
                Streamer.Queue.Enqueue(Music[0]);
            }
        }
    }
}
