namespace Smpsp
{
    public class SignInCodeReply
    {
        public string Token { get; set; } = string.Empty;
        public User User { get; set; } = default!;
        public long UnixTimestampExpirationDate { get; set; }
    }
}
