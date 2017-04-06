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
                var Client = await User.VoiceChannel.ConnectAsync();
                Client.Disconnected += async delegate
                {
                    if (Connection.Client == Client)
                    {
                        Out = null;
                    }
                };

                Connection.Client = Client;
            }
        }

        internal static async Task<AudioOutStream> GetStream()
        {
            if (Client != null && Out == null)
            {
                Out = Client.CreateDirectPCMStream(AudioApplication.Music, 2880, 2);
            }

            return Out;
        }
    }
}
