using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mirai
{
    class Bot
    {
        internal static DiscordSocketClient Client;
        internal static ulong ChannelId;
        static SocketTextChannel ChannelCached;

        internal static SocketSelfUser User
        {
            get
            {
                return Client.CurrentUser;
            }
        }

        internal static SocketTextChannel Channel
        {
            get
            {
                return ChannelCached ?? (ChannelCached = Client.Guilds.SelectMany(x => x.TextChannels).Where(x => x.Id == ChannelId).First());
            }
        }

        internal static async Task JoinOwner()
        {
            var Users = Client.Guilds.SelectMany(x => x.Users);
            foreach (var User in Users)
                if (User.Id == Mirai.User.Owner && User.VoiceChannel != null)
                {
                    await Audio.Connection.JoinSame(User as IGuildUser);
                    break;
                }
        }

        internal static async Task Login(string Token, ulong Channel)
        {
            ChannelId = Channel;

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

            await Client.LoginAsync(TokenType.Bot, Token);
            await Client.StartAsync();

            await Waiter.Task;
        }

        static int TTSWaiter = 0;

        internal static async Task<RestUserMessage> SendTTS(string Text, Embed Embed = null, RequestOptions Options = null)
        {
            var TTS = Interlocked.Increment(ref TTSWaiter) < 4; //Max 3 simultaneously
            var Message = await Channel.SendMessageAsync(Text, TTS, Embed, Options);
            Interlocked.Decrement(ref TTSWaiter);

            return Message;
        }

        internal static async Task Logout()
        {
            await Client?.StopAsync();
            await Client?.LogoutAsync();
        }
    }
}
