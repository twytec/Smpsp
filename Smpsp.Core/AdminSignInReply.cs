namespace Smpsp
{
    public class AdminSignInReply
    {
        public string Token { get; set; } = string.Empty;
        public long UnixTimestampExpirationDate { get; set; }
    }
}
