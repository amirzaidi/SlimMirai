using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace MusicSearch
{
    public class API
    {
        public static string YouTube
        {
            set
            {
                SongRequest.YT = new YouTubeService(new BaseClientService.Initializer
                {
                    ApiKey = value
                });
            }
        }

        public static string SoundCloud
        {
            set
            {
                SongRequest.SC = $"client_id={value}";
            }
        }
    }
}
