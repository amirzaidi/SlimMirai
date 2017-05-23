using Discord;
using Discord.Audio;
using System.Threading.Tasks;

namespace Mirai.Audio
{
    class Connection
    {
        private static IAudioClient Client;
        private static AudioOutStream Out;

        internal static async Task<IVoiceChannel> JoinSame(IGuildUser User)
        {
            if (User.VoiceChannel != null)
                Client = await User.VoiceChannel.ConnectAsync(Peer =>
                {
                    Peer.StreamCreated += PeerJoinVoice;
                    Peer.StreamDestroyed += PeerLeaveVoice;
                    Peer.Disconnected += async delegate
                    {
                        Out?.Dispose();
                        Out = null;
                    };
                });

            return User.VoiceChannel;
        }

        private static async Task PeerJoinVoice(ulong s, AudioInStream e)
        {
            Speech.RestartListenService(s, e);
        }
        
        private static async Task PeerLeaveVoice(ulong s)
        {
            Speech.StopListenService(s);
        }

        internal static async Task<AudioOutStream> GetStream()
        {
            if (Client != null && Out == null)
                Out = Client.CreateDirectPCMStream(AudioApplication.Music, 2880);

            return Out;
        }
    }
}
