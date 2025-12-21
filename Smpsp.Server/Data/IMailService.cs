namespace Smpsp.Server.Data
{
    public interface IMailService
    {
        public ValueTask<bool> TrySendSignInCodeMessage(User user, string code);
        public ValueTask<bool> TrySendTestMessage(string toEmail);
    }
}
