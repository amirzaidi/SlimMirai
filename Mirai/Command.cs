using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mirai
{
    class Command
    {
        static Dictionary<string, Command> List;

        internal static void Load(string Mention)
        {
            List = new Dictionary<string, Command>
            {
                { Mention, new Command(Conversation.Commands.Question, 1) },
                { "add", new Command(Music.Commands.Add, 1) }
            };
        }

        internal static Command Get(string KeyWord, int Rank)
        {
            if (List?.ContainsKey(KeyWord) ?? false)
            {
                var Command = List[KeyWord];
                if (Command.Rank <= Rank)
                {
                    return Command;
                }
            }

            return null;
        }

        Func<string, SocketMessage, Task> Handler;
        int Rank;

        Command(Func<string, SocketMessage, Task> Handler, int Rank)
        {
            this.Handler = Handler;
            this.Rank = Rank;
        }

        internal async void Invoke(string s, SocketMessage e)
        {
            try
            {
                await Handler(s, e);
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex);
            }
        }
    }
}
