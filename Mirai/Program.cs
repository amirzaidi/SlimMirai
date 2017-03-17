using MusicSearch;
using System;
using System.Threading.Tasks;

namespace Mirai
{
    class Program
    {
        internal const string Bot = "MTczNzM0NjY3MjkwMjc5OTM2.Cf-Vew.KXzRYYUt6i2hXBAKhm1MM0hmcyk";
        internal const ulong TextChannel = 74785335908773888;

        static void Main(string[] args)
            => Boot().GetAwaiter().GetResult();

        static async Task Boot()
        {
            Console.Title = "Mirai";

            SongRequest.YouTube = "AIzaSyAVrXiAHfLEbQbNJP80zbTuW2jL0wuEigQ";
            SongRequest.SoundCloud = "5c28ed4e5aef8098723bcd665d09041d";

            await Account.Login();

            while (true)
            {
                //Do Task
                await Task.Delay(1000);
            }
        }
    }
}