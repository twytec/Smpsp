namespace Smpsp
{
    public class PostMedia
    {
        public const string ConvertTag = "_";

        public string File { get; set; } = string.Empty;
        public string ContentAlt { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public bool MustBeConverted { get; set; }
        public bool IsImage { get; set; }

        public string GetMimeType()
        {
            return Extension switch
            {
                ".mp4" => "video/mp4",
                _ => "video/webm",
            };
        }
    }
}
