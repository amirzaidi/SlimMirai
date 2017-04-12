using Discord;
using Discord.Audio;
using System.Threading.Tasks;

namespace Mirai.Audio
{
    class Connection
    {
        private static IAudioClient Client;
        private static AudioOutStream Out;

        internal static async Task JoinSame(IGuildUser User)
        {
            if (User.VoiceChannel != null)
            {
                Connection.Client = await User.VoiceChannel.ConnectAsync(Client =>
                {
                    Client.StreamCreated += UserJoinVoice;
                    Client.StreamDestroyed += UserLeaveVoice;
                    Client.Disconnected += async delegate
                    {
                        if (Connection.Client == Client)
                        {
                            Out = null;
                        }
                    };
                });
            }
        }

        private static async Task UserJoinVoice(ulong s, AudioInStream e)
        {
            Speech.RestartListenService(s, e);
        }
        
        private static async Task UserLeaveVoice(ulong s)
        {
            Speech.StopListenService(s);
        }

        internal static async Task<AudioOutStream> GetStream()
        {
            if (Client != null && Out == null)
            {
                Out = Client.CreateDirectPCMStream(AudioApplication.Music, 2880);
            }

            return Out;
        }
    }
}
