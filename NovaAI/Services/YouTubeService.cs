using System.Text.Json;
using NovaAI.Models;

namespace NovaAI.Services
{
    public class YouTubeService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public YouTubeService(HttpClient http, IConfiguration config)
        {
            _http = http;
            // json dosyasını devre dışı bırakıp şifreyi direkt koda çakıyoruz:
            _apiKey = "apikey";
        }

        // ── Kanal ID'yi URL'den çıkar ──
        public async Task<string> ResolveChannelIdAsync(string input)
        {
            input = input.Trim().TrimEnd('/');

            if (input.StartsWith("UC") && input.Length == 24)
                return input;

            string handle = "";

            if (input.Contains("/@"))
                handle = input.Split("/@")[1].Split('/')[0];
            else if (input.Contains("/channel/"))
                return input.Split("/channel/")[1].Split('/')[0];
            else if (input.StartsWith("@"))
                handle = input.TrimStart('@');
            else
                handle = input;

            var url = $"https://www.googleapis.com/youtube/v3/channels" +
                      $"?part=id&forHandle={handle}&key={_apiKey}";

            var res = await _http.GetStringAsync(url);
            var json = JsonDocument.Parse(res);
            var items = json.RootElement.GetProperty("items");

            if (items.GetArrayLength() == 0)
                throw new Exception("Kanal bulunamadı.");

            return items[0].GetProperty("id").GetString() ?? string.Empty;
        }

        // ── Kanal Analizi ──
        public async Task<ChannelAnalysis> GetChannelAnalysisAsync(string channelId)
        {
            var analysis = new ChannelAnalysis { ChannelId = channelId };

            var statsUrl = $"https://www.googleapis.com/youtube/v3/channels" +
                           $"?part=snippet,statistics&id={channelId}&key={_apiKey}";

            var statsRes = await _http.GetStringAsync(statsUrl);
            var statsJson = JsonDocument.Parse(statsRes);
            var channel = statsJson.RootElement.GetProperty("items")[0];
            var snippet = channel.GetProperty("snippet");
            var stats = channel.GetProperty("statistics");

            analysis.ChannelName = snippet.GetProperty("title").GetString() ?? "";
            analysis.Description = snippet.GetProperty("description").GetString() ?? "";
            analysis.Thumbnail = snippet.GetProperty("thumbnails")
                                          .GetProperty("high")
                                          .GetProperty("url").GetString() ?? "";

            analysis.Subscribers = long.TryParse(
                stats.TryGetProperty("subscriberCount", out var sc)
                    ? sc.GetString() : "0", out var s) ? s : 0;

            analysis.TotalViews = long.TryParse(
                stats.TryGetProperty("viewCount", out var vc)
                    ? vc.GetString() : "0", out var v) ? v : 0;

            analysis.VideoCount = long.TryParse(
                stats.TryGetProperty("videoCount", out var vdc)
                    ? vdc.GetString() : "0", out var vd) ? vd : 0;

            analysis.TopVideos = await GetTopVideosAsync(channelId);

            if (analysis.TopVideos.Any())
                analysis.Comments = await GetCommentsAsync(
                    analysis.TopVideos[0].VideoId);

            return analysis;
        }

        // ── Top 10 Video ──
        private async Task<List<VideoInfo>> GetTopVideosAsync(string channelId)
        {
            var searchUrl = $"https://www.googleapis.com/youtube/v3/search" +
                            $"?part=snippet&channelId={channelId}" +
                            $"&maxResults=10&order=viewCount&type=video&key={_apiKey}";

            var searchRes = await _http.GetStringAsync(searchUrl);
            var searchJson = JsonDocument.Parse(searchRes);
            var items = searchJson.RootElement.GetProperty("items");

            var videoIds = new List<string>();
            foreach (var item in items.EnumerateArray())
            {
                var id = item.GetProperty("id")
                             .GetProperty("videoId").GetString();
                if (id != null) videoIds.Add(id);
            }

            if (!videoIds.Any()) return new();

            var statsUrl = $"https://www.googleapis.com/youtube/v3/videos" +
                           $"?part=snippet,statistics&id={string.Join(",", videoIds)}" +
                           $"&key={_apiKey}";

            var statsRes = await _http.GetStringAsync(statsUrl);
            var statsJson = JsonDocument.Parse(statsRes);

            var videos = new List<VideoInfo>();
            foreach (var item in statsJson.RootElement
                .GetProperty("items").EnumerateArray())
            {
                var snip = item.GetProperty("snippet");
                var vstat = item.GetProperty("statistics");

                videos.Add(new VideoInfo
                {
                    VideoId = item.GetProperty("id").GetString() ?? "",
                    Title = snip.GetProperty("title").GetString() ?? "",
                    Thumbnail = snip.GetProperty("thumbnails")
                                      .GetProperty("medium")
                                      .GetProperty("url").GetString() ?? "",
                    PublishedAt = snip.GetProperty("publishedAt")
                                      .GetString() ?? "",
                    ViewCount = long.TryParse(
                        vstat.TryGetProperty("viewCount", out var vvc)
                            ? vvc.GetString() : "0", out var vv) ? vv : 0,
                    LikeCount = long.TryParse(
                        vstat.TryGetProperty("likeCount", out var vlc)
                            ? vlc.GetString() : "0", out var vl) ? vl : 0,
                    CommentCount = long.TryParse(
                        vstat.TryGetProperty("commentCount", out var vcc)
                            ? vcc.GetString() : "0", out var vc2) ? vc2 : 0,
                });
            }

            return videos.OrderByDescending(v => v.ViewCount).ToList();
        }

        // ── Yorumlar ──
        private async Task<List<string>> GetCommentsAsync(string videoId)
        {
            try
            {
                var url = $"https://www.googleapis.com/youtube/v3/commentThreads" +
                          $"?part=snippet&videoId={videoId}" +
                          $"&maxResults=50&order=relevance&key={_apiKey}";

                var res = await _http.GetStringAsync(url);
                var json = JsonDocument.Parse(res);

                var comments = new List<string>();
                foreach (var item in json.RootElement
                    .GetProperty("items").EnumerateArray())
                {
                    var text = item.GetProperty("snippet")
                                   .GetProperty("topLevelComment")
                                   .GetProperty("snippet")
                                   .GetProperty("textDisplay")
                                   .GetString();
                    if (text != null) comments.Add(text);
                }
                return comments;
            }
            catch { return new(); }
        }
    }
}