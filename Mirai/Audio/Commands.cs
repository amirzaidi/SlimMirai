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
        internal static SongType[] SearchTypes = new[] { SongType.Storage, SongType.SoundCloud, SongType.YouTube };
        static SemaphoreSlim Waiter = new SemaphoreSlim(1, 1);

        private static async Task<Song?> ResultAsync(string s)
        {
            var Music = await SongRequest.Search(s, SearchTypes);
            if (Music.Count != 0)
            {
                var Check = Music[0].Name.ToLower();
                var Split = s.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (Split.Count(x => Check.Contains(x)) * 2 > Split.Length)
                {
                    return Music[0];
                }
            }

            return null;
        }

        const string SearchFile = "Search.txt";
        
        internal static async Task Index(string s, SocketMessage e)
        {
            s = s.Replace("\r", "").Replace("\n", "");
            Logger.Log("Indexing " + s);

            var Result = await ResultAsync(s);
            if (Result != null)
            {
                var AllText = File.ReadAllText(SearchFile);
                var Pos = AllText.IndexOf("\r\n" + s + "\r\n");

                if (Pos == -1)
                {
                    await Waiter.WaitAsync();
                    using (var Open = File.AppendText(SearchFile))
                    {
                        await Open.WriteAsync(s + "\r\n");
                    }
                    Waiter.Release();

                    e.Channel.SendMessageAsync(s + " will now find " + Result?.Title);
                    SpeechEngine.Invalidate();
                }
            }
        }

        internal static async Task Deindex(string s, SocketMessage e)
        {
            s = s.Replace("\r", "").Replace("\n", "");

            Logger.Log("Deindexing " + s);
            if (await ResultAsync(s) != null)
            {
                var AllText = File.ReadAllText(SearchFile);
                var Pos = AllText.IndexOf("\r\n" + s + "\r\n");

                if (Pos != -1)
                {
                    await Waiter.WaitAsync();
                    File.WriteAllText(SearchFile, AllText.Replace("\r\n" + s + "\r\n", "\r\n"));
                    Waiter.Release();

                    e.Channel.SendMessageAsync(s + " has been removed");
                    SpeechEngine.Invalidate();
                }
            }
        }

        internal static async Task Join(string s, SocketMessage e)
        {
            var Voice = await Connection.JoinSame(e.Author as IGuildUser);
            Formatting.Update("Hello, " + Voice?.Name ?? "unknown channel");
        }

        internal static async Task Next(ulong User, Queue<string> Args)
        {
            Streamer.Next();
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

        internal static async Task Add(ulong User, Queue<string> Args)
        {
            var Types = new SongType[SearchTypes.Length];
            SearchTypes.CopyTo(Types, 0);

            var Words = Args.ToArray();
            if (Words.Length > 2 && Words[Words.Length - 2] == "from")
            {
                var Source = Words[Words.Length - 1];
                foreach (var Type in Types)
                {
                    if (Type.ToString() == Source)
                    {
                        Types = new[] { Type };
                        Array.Resize(ref Words, Words.Length - 2);
                        break;
                    }
                }
            }

            var Text = string.Join(" ", Words);
            Logger.Log("Adding music (voice): " + Text);
            var Music = await SongRequest.Search(Text, Types, true);
            if (Music.Count != 0)
            {
                var Playing = Streamer.Queue.IsPlaying;
                var Place = Streamer.Queue.Enqueue(Music[0]);
                Logger.Log($"Added {Music[0].Title} at {Place}");
                if (Playing)
                    Formatting.Update($"Added {Music[0].Title} at {Place}");
            }
        }

        internal static async Task Playlist(ulong User, Queue<string> Args)
        {
            if (Streamer.Queue.IsPlaying)
            {
                var Titles = Streamer.Queue.Titles;
                for (int i = 0; i < Titles.Length; i++)
                {
                    Titles[i] = $"{Titles[i]} at {i + 1}";
                }

                Formatting.Update("Playing " + Streamer.Queue.Playing.Title + "\n" + string.Join("\n", Titles));
            }
        }

        internal static async Task Set(ulong User, Queue<string> Args)
        {
            Args.Dequeue();
            var Var = Args.Dequeue();
            Args.Dequeue();
            var Value = double.Parse(Args.Dequeue());

            if (Var == "volume" && Value <= 10)
                Filter.Volume = Value / 10;
            else if (Var == "pitch" && Value >= 5 && Value <= 20)
                Filter.Tone = Value / 10;
            else
                return;

            Streamer.ReloadSong();
        }
    }
}
