using Google.Apis.YouTube.v3;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace MusicSearch
{
    public class SongRequest
    {
        internal static YouTubeService YT;
        internal static string SC;
        private static readonly Regex YoutubeRegex = new Regex(@"youtu(?:\.be|be\.com)/(?:(.*)v(/|=)|(.*/)?)([a-zA-Z0-9-_]+)", RegexOptions.IgnoreCase);

        public static Func<string, Task<string>> GetTelegramUrl;

        public static async Task<string> StreamUrl(Song Song, bool AllFormats = true)
        {
            if (Song.Type == SongType.YouTube)
            {
                YoutubeClient.TryParseVideoId(Song.Url, out string Id);

                var Client = new YoutubeClient();
                var Vid = await Client.GetVideoInfoAsync(Id);
                MixedStreamInfo Max = null;
                foreach (var V in Vid.MixedStreams)
                    if (Max == null || Max.VideoQuality < V.VideoQuality)
                        Max = V;

                return Max?.Url ?? string.Empty;
            }
            else if (Song.Type == SongType.SoundCloud)
            {
                var SCRes = await($"http://api.soundcloud.com/resolve?url={Song.Url}&{SC}").WebResponseRetryLoop();
                if (SCRes != string.Empty && SCRes.StartsWith("{\"kind\":\"track\"")) 
                {
		            var Parse = JObject.Parse(SCRes);

                    if (Parse["downloadable"] != null && (bool)Parse["downloadable"] == true)
                        return $"{Parse["download_url"]}?{SC}";

                    return $"{Parse["stream_url"]}?{SC}";
                }
            }
            else if (Song.Type == SongType.Telegram)
                return await GetTelegramUrl(Song.Url);

            return Song.Url;
        }

        public static async Task<List<Song>> Search(object ToSearch, SongType[] Sources, bool ReturnAtOnce = false)
        {
            var Query = ((string)ToSearch).Trim();
            var Results = new List<Song>();

            Match Match;
            if (Sources.Contains(SongType.YouTube) && (Match = YoutubeRegex.Match(Query)).Success)
            {
                var ResultData = await YouTubeParse(Match.Groups[4].Value);
                if (ResultData != null)
                    Results.Add((Song)ResultData);
            }
            else if (Sources.Contains(SongType.SoundCloud) && Regex.IsMatch(Query, "(.*)(soundcloud.com|snd.sc)(.*)"))
            {
                var SCRes = await ($"http://api.soundcloud.com/resolve?url={Query}&{SC}").WebResponseRetryLoop();
                if (SCRes != string.Empty && SC.StartsWith("{\"kind\":\"track\""))
                    Results.Add(SoundCloudParse(JToken.Parse(SCRes)));
            }
            /*else if (Sources.Contains(SongType.Storage) && Uri.TryCreate(Query, UriKind.Absolute, out Uri Url))
            {
                Results.Add(new Song
                {
                    Name = Path.GetFileNameWithoutExtension(Url.LocalPath),
                    Desc = $"Remote {Path.GetExtension(Url.LocalPath)} file",
                    Url = Query,
                    Type = SongType.Storage
                });
            }*/

            if (Sources.Contains(SongType.Storage) && Query.Length >= 3)
            {
                var SplitQuery = Query.Split(' ');
                var Range = SongRequestLocal.GetFiles()
                    .Where(
                        x => x.Name.Length >= Query.Length && SplitQuery.All(y => x.Name.IndexOf(y, StringComparison.OrdinalIgnoreCase) >= 0)
                    )
                    .Select(x => new Song
                    {
                        Name = x.Name,
                        Desc = $"{x.Extension} file at {x.Dir}",
                        Url = x.Path,
                        Type = SongType.Storage
                    });

                if (Range.Count() > 3)
                    Range = Range.Take(3);

                Results.AddRange(Range);
            }

            if (!ReturnAtOnce || Results.Count == 0)
            {
                Task<string> SCReq = null;

                if (Sources.Contains(SongType.SoundCloud))
                    SCReq = $"http://api.soundcloud.com/tracks/?{SC}&q={UrlEncoder.Default.Encode(Query)}".WebResponseRetryLoop();

                if (Sources.Contains(SongType.YouTube))
                {
                    var ListRequest = YT.Search.List("snippet");
                    ListRequest.Q = Query;
                    ListRequest.MaxResults = 3;
                    ListRequest.Type = "video";
                    foreach (var Result in (await ListRequest.ExecuteAsync()).Items)
                    {
                        var ResultData = await YouTubeParse(Result.Id.VideoId);
                        if (ResultData != null)
                            Results.Add((Song)ResultData);
                    }
                }

                if (SCReq != null)
                {
                    int i = 0;
                    foreach (var Response in JArray.Parse(await SCReq))
                    {
                        if (++i > 3)
                            break;

                        Results.Add(SoundCloudParse(Response));
                    }
                }
            }

            return Results;
        }

        private static async Task<Song?> YouTubeParse(string VideoId)
        {
            var Search = YT.Videos.List("contentDetails,snippet");
            Search.Id = VideoId;

            var Videos = await Search.ExecuteAsync();
            var Result = Videos.Items.FirstOrDefault();

            if (Result != null)
            {
                var Desc = Result.Snippet.Description;
                if (Desc.Length == 0)
                    Desc = "No description";

                return new Song
                {
                    Name = Result.Snippet.Title,
                    Desc = $"{XmlConvert.ToTimeSpan(Result.ContentDetails.Duration).ToHMS()} on YouTube | {Desc}",
                    Url = $"http://www.youtube.com/watch?v={Search.Id}",
                    Type = SongType.YouTube,
                    ThumbNail = Result.Snippet.Thumbnails.Maxres?.Url ?? Result.Snippet.Thumbnails.Default__?.Url
                };
            }

            return null;
        }

        private static string StripHtml(string In)
        {
            return Regex.Replace(In, @"<[^>]*>", string.Empty);
        }

        private static Song SoundCloudParse(JToken Response)
        {
            var Desc = Response["description"].ToString().Replace("\n", " ");
            if (Desc.Length == 0)
                Desc = Response["genre"].UcWords().ToString();

            var Thumb = Response["artwork_url"].ToString();
            if (Thumb == string.Empty)
                Thumb = "http://i.imgur.com/eRaxycY.png";

            return new Song
            {
                Name = Response["title"].ToString(),
                Desc = $"{new TimeSpan(0, 0, 0, 0, Response["duration"].ToObject<int>()).ToHMS()} on SoundCloud | {StripHtml(Desc.Trim())}",
                Url = Response["uri"].ToString(),
                Type = SongType.SoundCloud,
                ThumbNail = Thumb
            };
        }
    }
}
