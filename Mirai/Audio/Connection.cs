using Discord;
using Discord.Audio;
using System.Threading.Tasks;

namespace Mirai.Audio
{
    class Connection
    {
        internal static async Task<IVoiceChannel> JoinSame(IGuildUser User)
        {
            var Channel = User?.VoiceChannel;
            if (Channel != null)
            {
                Streamer.StopPlayback();

                var Client = await Channel.ConnectAsync(Peer =>
                {
                    Peer.StreamCreated += async (s, e) => Speech.StartListenService(s, e);
                    Peer.StreamDestroyed += async s => Speech.StopListenService(s);
                });

                Streamer.StartPlayback(Client.CreateDirectPCMStream(AudioApplication.Music, 2880));
            }

            return Channel;
        }
    }
}
