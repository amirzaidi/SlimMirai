using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Mirai
{
    class Account
    {
        internal static DiscordSocketClient Client;

        internal static async Task Login()
        {
            Client = new DiscordSocketClient();
            
            Client.Connected += Events.Connected;
            Client.MessageReceived += Events.MessageReceived;
            Client.Log += Events.Log;
            Client.Disconnected += Events.Disconnected;

            await Client.LoginAsync(TokenType.Bot, Program.Bot);
            await Client.StartAsync();
        }

        internal static async Task Logout()
        {
            await Client?.StopAsync();
            await Client?.LogoutAsync();
        }
    }
}
