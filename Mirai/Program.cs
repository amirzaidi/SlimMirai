using MusicSearch;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mirai
{
    class Program
    {
        internal const string Token = "MTczNzM0NjY3MjkwMjc5OTM2.Cf-Vew.KXzRYYUt6i2hXBAKhm1MM0hmcyk";
        internal const ulong TextChannel = 74785335908773888;

        static void Main(string[] args)
            => Boot().GetAwaiter().GetResult();

        static async Task Boot()
        {
            Console.Title = "Slim Mirai";

            SongRequest.YouTube = "AIzaSyAVrXiAHfLEbQbNJP80zbTuW2jL0wuEigQ";
            SongRequest.SoundCloud = "5c28ed4e5aef8098723bcd665d09041d";

            await Bot.Login();
            Command.Load(Bot.Client.CurrentUser.Mention.Replace("!", ""));
            Audio.Streamer.Restart();

            var Owner = Bot.Client.Guilds.SelectMany(x => x.Users).First(x => User.GetRank(x.Id) == 3);
            Audio.Connection.JoinSame(Owner as Discord.IGuildUser);

            while (true)
            {
                //Do Task
                await Task.Delay(50);

                Console.Title = $"Slim Mirai | {Audio.SpeechEngine.Count} Engines";
            }
        }
    }
}