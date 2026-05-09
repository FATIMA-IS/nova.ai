namespace NovaAI.Models
{
    public class ChannelAnalysis
    {
        public string ChannelId { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long Subscribers { get; set; }
        public long TotalViews { get; set; }
        public long VideoCount { get; set; }
        public List<VideoInfo> TopVideos { get; set; } = new();
        public List<string> Comments { get; set; } = new();
    }

    public class VideoInfo
    {
        public string VideoId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        public long ViewCount { get; set; }
        public long LikeCount { get; set; }
        public long CommentCount { get; set; }
        public string PublishedAt { get; set; } = string.Empty;
    }
}