using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MusicSearch;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System;

namespace Mirai.Audio
{
    static class Commands
    {
        static SemaphoreSlim Waiter = new SemaphoreSlim(1, 1);

        internal static async Task Add(string s, SocketMessage e)
        {
            Logger.Log("Adding music: " + s);
            var Music = await SongRequest.Search(s);
            if (Music.Count != 0)
            {
                Streamer.Queue.Enqueue(Music[0]);

                var Check = Music[0].FullName.ToLower();
                var Split = s.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (Split.Count(x => Check.Contains(x)) * 2 > Split.Length)
                {
                    await Waiter.WaitAsync();

                    using (var Open = File.AppendText("Search.txt"))
                    {
                        await Open.WriteAsync(s + "\r\n");
                    }

                    Waiter.Release();
                    SpeechEngine.Invalidate();
                }
            }
        }

        internal static async Task DirectAdd(string s, SocketMessage e)
        {
            Logger.Log("Adding music: " + s);
            var Music = await SongRequest.Search(s);
            if (Music.Count != 0)
            {
                Streamer.Queue.Enqueue(Music[0]);
            }
        }

        internal static async Task Volume(string s, SocketMessage e)
        {
            if (s != string.Empty)
            {
                s = $"volume={s}";
            }

            Streamer.Filter = s;
        }

        internal static async Task Join(string s, SocketMessage e)
        {
            Connection.JoinSame(e.Author as IGuildUser);
            Formatting.Update("Hello, " + e.Channel.Name);
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

        internal static async Task Skip(ulong User, Queue<string> Args)
        {
            Streamer.Skip?.Cancel();
        }

        internal static async Task Remove(ulong User, Queue<string> Args)
        {
            if (ushort.TryParse(Args.Dequeue(), out ushort Result) && Streamer.Queue.TryRemove((ushort)(Result - 1), out Song Song))
            {
                Formatting.Update($"Removed {Song.Title}");
            }
        }

        internal static async Task Move(ulong User, Queue<string> Args)
        {
            var From = Args.Dequeue();
            Args.Dequeue();
            var To = Args.Dequeue();

            if (int.TryParse(From, out int FromInt) && int.TryParse(To, out int ToInt) && Streamer.Queue.TryPush(FromInt - 1, ToInt - 1, out Song Song))
            {
                Formatting.Update($"Moved {Song.Title} to {ToInt}");
            }
        }

        internal static async Task Play(ulong User, Queue<string> Args)
        {
            var Text = string.Join(" ", Args);
            Logger.Log("Adding music (voice): " + Text);
            var Music = await SongRequest.Search(Text, true);
            if (Music.Count != 0)
            {
                var Playing = Streamer.Queue.IsPlaying;
                var Place = Streamer.Queue.Enqueue(Music[0]);
                if (Playing)
                {
                    Formatting.Update($"Added {Music[0].Title} at {Place}");
                }
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

                Formatting.Update(string.Join("\n", Titles));
            }
        }
    }
}
