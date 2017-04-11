using Discord.WebSocket;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mirai
{
    class CommandFunc<T>
    {
        Func<string, T, Task> Handler;
        internal int Rank;

        internal CommandFunc(Func<string, T, Task> Handler, int Rank)
        {
            this.Handler = Handler;
            this.Rank = Rank;
        }

        internal async void Invoke(string s, T e)
        {
            try
            {
                await Handler(s, e);
            }
            catch (Exception Ex)
            {
                Logger.Log(Ex);
            }
        }
    }

    class Command
    {
        static Dictionary<string, CommandFunc<SocketMessage>> Typed;
        static Dictionary<string, CommandFunc<ulong>> Voiced;

        internal static void Load(string Mention)
        {
            Typed = new Dictionary<string, CommandFunc<SocketMessage>>
            {
                { Mention, new CommandFunc<SocketMessage>(Conversation.Commands.Question, 1) },
                { "add", new CommandFunc<SocketMessage>(Audio.Commands.Add, 1) },
                { "join", new CommandFunc<SocketMessage>(Audio.Commands.Join, 2) }
            };

            Voiced = new Dictionary<string, CommandFunc<ulong>>
            {
                { "volgende", new CommandFunc<ulong>(Audio.Commands.Skip, 1) },
                { "verwijder", new CommandFunc<ulong>(Audio.Commands.Remove, 1) },
                { "verplaats", new CommandFunc<ulong>(Audio.Commands.Move, 1) },
                { "local", new CommandFunc<ulong>(Audio.Commands.Local, 1) },
            };

            var NumberChoices = new Choices();
            for (int i = 1; i <= 30; i++)
            {
                NumberChoices.Add(i.ToString());
            }

            var Volgende = new GrammarBuilder("volgende");
            var Verwijder = new GrammarBuilder("verwijder");
            Verwijder.Append(NumberChoices);
            var Verplaats = new GrammarBuilder("verplaats");
            Verplaats.Append(NumberChoices);
            Verplaats.Append("naar");
            Verplaats.Append(NumberChoices);
            var Local = new GrammarBuilder("local")
            {
                Culture = new System.Globalization.CultureInfo("en-US")
            };
            var LocalFiles = new Choices();
            var Regex = new Regex("[^a-zA-Z0-9 ]");
            foreach (var File in MusicSearch.SongRequestLocal.GetFiles())
            {
                LocalFiles.Add(Regex.Replace(File.Name, "").Replace("  ", " "));
            }
            Local.Append(LocalFiles);

            Audio.SpeechEnginePool.GrammarBuilder = new GrammarBuilder(new Choices(Volgende, Verwijder, Verplaats, Local))
            {
                Culture = Audio.SpeechEnginePool.Culture
            };
        }

        internal static CommandFunc<SocketMessage> Get(string KeyWord, int Rank)
        {
            if (Typed?.ContainsKey(KeyWord) ?? false)
            {
                var Command = Typed[KeyWord];
                if (Command.Rank <= Rank)
                {
                    return Command;
                }
            }

            return null;
        }

        internal static CommandFunc<ulong> GetVoice(string KeyWord, int Rank)
        {
            if (Voiced?.ContainsKey(KeyWord) ?? false)
            {
                var Command = Voiced[KeyWord];
                if (Command.Rank <= Rank)
                {
                    return Command;
                }
            }

            return null;
        }
    }
}
