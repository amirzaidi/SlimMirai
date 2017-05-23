using MusicSearch;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Mirai
{
    class Program
    {
        static void Main(string[] args)
            => Boot().GetAwaiter().GetResult();

        static async Task Boot()
        {
            Console.Title = "Slim Mirai";
            
            API.YouTube = Config("YouTubeToken");
            API.SoundCloud = Config("SoundCloudToken");

            await Bot.Login(Config("DiscordToken"), ulong.Parse(Config("DiscordChannel")));
            Command.Load(Bot.User.Mention.Replace("!", ""));
            Audio.Streamer.Restart();

            var Owner = Bot.Client.Guilds.SelectMany(x => x.Users).First(x => User.GetRank(x.Id) == 3);
            await Audio.Connection.JoinSame(Owner as Discord.IGuildUser);

            while (true)
            {
                await Task.Delay(50);
                Console.Title = $"Slim Mirai | {Audio.SpeechEngine.Count} Engines";
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