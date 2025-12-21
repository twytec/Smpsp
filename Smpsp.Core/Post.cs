namespace Smpsp
{
    public class Post : IEquatable<Post?>
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public long CreatedUnixTimestamp { get; set; }
        public long EndOfVoting { get; set; }
        public PostStatus Status { get; set; }
        public CreationStatus CreationStatus { get; set; }
        public string[] Hashtags { get; set; } = [];
        public List<PostMedia> Medias { get; set; } = [];
        public List<PostVoting> Votings { get; set; } = [];
        public List<PostVeto> Vetoes { get; set; } = [];

        public override bool Equals(object? obj)
        {
            return Equals(obj as Post);
        }

        public bool Equals(Post? other)
        {
            return other is not null &&
                   Id == other.Id &&
                   UserId == other.UserId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, UserId);
        }
    }

    public enum CreationStatus
    {
        Complete = 0,
        CreateZip = 1,
        Convert = 2
    }
}
