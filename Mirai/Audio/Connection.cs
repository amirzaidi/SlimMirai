using Discord;
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
                Streamer.Stop();
                
                (await Channel.ConnectAsync()).Dispose(); //Disconnect first
                var Client = await Channel.ConnectAsync(Peer =>
                {
                    Peer.StreamCreated += async (s, e) => Speech.StartListenService(s, e);
                    Peer.StreamDestroyed += async s => Speech.StopListenService(s);
                });

                Streamer.Start(Client);
            }

            return Channel;
        }
    }
}
