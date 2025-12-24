namespace Smpsp
{
    public class MySettings
    {
        public int MaxRequestBodySize { get; set; } = 1_048_576;

        public string[] SupportedImageExtension { get; set; } = [".bmp", ".gif", ".jpeg", ".jpg", ".png", ".wbmp", ".webp", ".pkm", ".ktx", ".astc", ".dng", ".heif", ".heic", ".avif"];
        public string[] SupportedVideoExtension { get; set; } = [".mpeg", ".mp4", ".ogg", ".webm", ".mov", ".mkv", ".avi"];
        public string[] ImageConvertToPng { get; set; } = [".bmp", ".pkm", ".ktx", ".astc", ".dng", ".heif", ".heic", ".avif"];
        public string[] VideoConvertToWebm { get; set; } = [".mpeg", ".ogg", ".mov", ".mkv", ".avi"];
        public int MaxAllowedImageSize { get; set; } = 10_485_760;
        public int MaxAllowedVideoSize { get; set; } = 104_857_600;

        public string[] Hashtags { get; set; } = [];

        public int DefaultVotingPeriodInHours { get; set; } = 48;
        public int DeletePostAfterDays { get; set; } = 30;

        public string AdminName { get; set; } = "admin";
        public string AdminPassword { get; set; } = "admin";
        public string LanguageCode { get; set; } = "en-us";

        public int SignInExpirationDays { get; set; } = 30;
        public int SignInCodeExpirationHours { get; set; } = 1;
        public string IssuerSigningKey { get; set; } = string.Empty;

        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpEmail { get; set; } = string.Empty;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;

        public bool FFmpegGlobal { get; set; } = true;

        public void MergeFrom(MySettings other)
        {
            MaxRequestBodySize = other.MaxRequestBodySize;
            SupportedImageExtension = other.SupportedImageExtension;
            SupportedVideoExtension = other.SupportedVideoExtension;
            VideoConvertToWebm = other.VideoConvertToWebm;
            ImageConvertToPng = other.ImageConvertToPng;

            MaxAllowedImageSize = other.MaxAllowedImageSize;
            MaxAllowedVideoSize = other.MaxAllowedVideoSize;

            Hashtags = other.Hashtags;

            DefaultVotingPeriodInHours = other.DefaultVotingPeriodInHours;
            DeletePostAfterDays = other.DeletePostAfterDays;

            AdminName = other.AdminName;
            AdminPassword = other.AdminPassword;
            LanguageCode = other.LanguageCode;

            SignInExpirationDays = other.SignInExpirationDays;
            SignInCodeExpirationHours = other.SignInCodeExpirationHours;
            IssuerSigningKey = other.IssuerSigningKey;

            SmtpServer = other.SmtpServer;
            SmtpPort = other.SmtpPort;
            SmtpEmail = other.SmtpEmail;
            SmtpUsername = other.SmtpUsername;
            SmtpPassword = other.SmtpPassword;

            FFmpegGlobal = other.FFmpegGlobal;

            if (SignInExpirationDays < 1)
                SignInExpirationDays = 1;

            if (SignInCodeExpirationHours < 1)
                SignInCodeExpirationHours = 1;
        }
    }
}
