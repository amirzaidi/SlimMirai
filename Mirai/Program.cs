using MusicSearch;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Mirai
{
    class Program
    {
        internal static bool Live = true;

        static void Main(string[] args)
            => Boot().GetAwaiter().GetResult();

        static async Task Boot()
        {
            Console.Title = "Slim Mirai";
            
            API.YouTube = Config("YouTubeToken");
            API.SoundCloud = Config("SoundCloudToken");

            Audio.SpeechEngine.Trigger = Config("Trigger").Split(' ');

            await Bot.Login(Config("DiscordToken"), ulong.Parse(Config("DiscordChannel")));
            Command.Load(Bot.User.Mention.Replace("!", ""));

            User.Owner = ulong.Parse(Config("Owner"));
            var Owner = Bot.Client.Guilds.SelectMany(x => x.Users).FirstOrDefault(x => x.Id == User.Owner);
            await Audio.Connection.JoinSame(Owner as Discord.IGuildUser);
            
            while (Live)
            {
                Console.Title = $"Slim Mirai | {(Audio.Streamer.Queue.IsPlaying ? $"Playback Speed {Audio.Streamer.PlaybackSpeed}" : "Silence" )}";
                await Task.Delay(16);
            }
        }

        static string Config(string Key)
        {
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[Key]))
            {
                Console.Write($"{Key}: ");
                var Value = Console.ReadLine();

                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                settings[Key].Value = Value;
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }

            return ConfigurationManager.AppSettings[Key];
        }
    }
}