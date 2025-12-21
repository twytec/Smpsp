using Smpsp.Server.Data;

namespace Smpsp.aTest.Server.Mocks
{
    public class MailService : IMailService
    {
        public string Code { get; set; } = string.Empty;

        public ValueTask<bool> TrySendSignInCodeMessage(User user, string code)
        {
            Code = code;
            return ValueTask.FromResult(true);
        }

        public ValueTask<bool> TrySendTestMessage(string toEmail)
        {
            throw new NotImplementedException();
        }
    }
}
