using Mirai.Audio;
using MusicSearch;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Mirai
{
    class Program
    {
        internal static TaskCompletionSource<bool> Shutdown = new TaskCompletionSource<bool>();

        static void Main(string[] args)
            => Boot().GetAwaiter().GetResult();

        static async Task Boot()
        {
            Logger.SetTitle("Booting");

            //Prevents other programs from causing stuttering
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            API.YouTube = Config("YouTube", "AIzaSyADrXiAHfLEbZbNJP60zbTuW2jL0wuEikQ");
            API.SoundCloud = Config("SoundCloud", "5c23ed4e5aef8098723bce665d06041d");

            SpeechEngine.Trigger = Config("Trigger", "music player").Split(' ');
            User.Owner = ulong.Parse(Config("Owner", "74779725393825792"));

            await Bot.Login(Config("Token", "MTczNzM0NjX3MjkwMjc5PTM3.Cf-Vew.KXzR..."), ulong.Parse(Config("Channel", "155608929122910208")));
            Command.Load(Bot.User.Mention.Replace("!", ""));

            await Bot.JoinOwner();
            await Shutdown.Task;
        }

        static string Config(string Key, string Example)
        {
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[Key]))
            {
                Console.Write($"{Key} (ex {Example}): ");
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