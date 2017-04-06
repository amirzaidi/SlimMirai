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

            var Waiter = new TaskCompletionSource<bool>();
            Client.Ready += async delegate
            {
                Waiter.SetResult(true);
            };

            await Client.LoginAsync(TokenType.Bot, Program.Bot);
            await Client.StartAsync();

            await Waiter.Task;
        }

        internal static async Task Logout()
        {
            await Client?.StopAsync();
            await Client?.LogoutAsync();
        }
    }
}
