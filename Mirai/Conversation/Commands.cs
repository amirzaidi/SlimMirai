using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Mirai.Conversation
{
    class Commands
    {
        public static async Task Question(string s, SocketMessage e)
        {
            Console.WriteLine("Asking question: " + s);
        }
    }
}
