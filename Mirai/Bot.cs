using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace Mirai
{
    class Bot
    {
        internal static DiscordSocketClient Client;
        static SocketTextChannel ChannelCached;
        internal static SocketTextChannel Channel()
        {
            return ChannelCached ?? (ChannelCached = Client.Guilds.SelectMany(x => x.TextChannels).Where(x => x.Id == Program.TextChannel).First());
        }

        internal static async Task Login()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                //LogLevel = LogSeverity.Debug
            });
            
            Client.Connected += Events.Connected;
            Client.MessageReceived += Events.MessageReceived;
            Client.Log += Events.Log;
            Client.Disconnected += Events.Disconnected;

            var Waiter = new TaskCompletionSource<bool>();
            Client.Ready += async delegate
            {
                Waiter.SetResult(true);
            };

            await Client.LoginAsync(TokenType.Bot, Program.Token);
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
