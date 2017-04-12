using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MusicSearch;
using System.Collections.Generic;

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

        internal static async Task Skip(ulong User, Queue<string> Args)
        {
            Streamer.Skip?.Cancel();
        }

        internal static async Task Remove(ulong User, Queue<string> Args)
        {
            if (ushort.TryParse(Args.Dequeue(), out ushort Result) && Streamer.Queue.TryRemove((ushort)(Result - 1), out Song Song))
            {
                Bot.Channel().SendMessageAsync($"Removed {Song.Title}", true);
            }
        }

        internal static async Task Move(ulong User, Queue<string> Args)
        {
            var From = Args.Dequeue();
            Args.Dequeue();
            var To = Args.Dequeue();

            if (int.TryParse(From, out int FromInt) && int.TryParse(To, out int ToInt) && Streamer.Queue.TryPush(FromInt - 1, ToInt - 1, out Song Song))
            {
                Bot.Channel().SendMessageAsync($"Moved {Song.Title} to {ToInt}", true);
            }
        }

        internal static async Task Play(ulong User, Queue<string> Args)
        {
            var Text = string.Join(" ", Args);
            Logger.Log("Adding local music: " + Text);
            var Music = await SongRequest.Search(Text, true);
            if (Music.Count != 0)
            {
                if (Streamer.Queue.IsPlaying)
                {
                    Bot.Channel().SendMessageAsync($"Added {Music[0].Title} at {Streamer.Queue.Count + 1}", true);
                }

                var Place = Streamer.Queue.Enqueue(Music[0]);
            }
        }

        internal static async Task Queue(ulong User, Queue<string> Args)
        {
            var Titles = Streamer.Queue.Titles;
            if (Titles.Length != 0)
            {
                for (int i = 0; i < Titles.Length; i++)
                {
                    Titles[i] = $"{Titles[i]} at {i + 1}";
                }

                Bot.Channel().SendMessageAsync(string.Join("\n", Titles), true);
            }
        }
    }
}
