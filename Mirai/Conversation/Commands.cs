using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mirai.Conversation
{
    class Commands
    {
        internal static async Task Question(string s, SocketMessage e)
        {
            Console.WriteLine("Asking question: " + s);
        }

        internal static async Task NoU(ulong User, Queue<string> Args)
        {
            Bot.Channel().SendMessageAsync("no u", true);
        }

        internal static async Task ImFine(ulong User, Queue<string> Args)
        {
            Bot.Channel().SendMessageAsync("I'm fine, thank you", true);
        }
    }
}
