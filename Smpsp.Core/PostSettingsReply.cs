namespace Smpsp
{
    public class PostSettingsReply
    {
        public string[] SupportedImageExtension { get; set; } = [];
        public string[] SupportedVideoExtension { get; set; } = [];
        public int MaxAllowedImageSize { get; set; }
        public int MaxAllowedVideoSize { get; set; }
        public int MaxRequestBodySize { get; set; }
        public string[] Hashtags { get; set; } = [];
        public int DefaultVotingPeriodInHours { get; set; }
    }
}
