using MimeKit;
using Smpsp.Server.Backgrounds;

namespace Smpsp.Server.Data
{
    public class MailService(IWebHostEnvironment _env, MySettingsService _mss, TranslationService _ts, MailQueue _mq) : IMailService
    {
        public async ValueTask<bool> TrySendSignInCodeMessage(User user, string code)
        {
            if (_env.IsDevelopment() && string.IsNullOrEmpty(_mss.Settings.SmtpServer))
            {
                Console.WriteLine(code);
                return true;
            }

            try
            {
                var i18n = _ts.GetTranslations(user.LanguageCode);
                MimeMessage msg = new();
                msg.From.Add(InternetAddress.Parse(_mss.Settings.SmtpEmail));
                msg.To.Add(InternetAddress.Parse(user.EMail));
                msg.Subject = i18n.SignInMailSubject;
                msg.Body = new TextPart($"{i18n.SignInMailMessage} {code}");

                await _mq.EnqueueAsync(msg);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async ValueTask<bool> TrySendTestMessage(string toEmail)
        {
            try
            {
                MimeMessage msg = new();
                msg.From.Add(InternetAddress.Parse(_mss.Settings.SmtpEmail));
                msg.To.Add(InternetAddress.Parse(toEmail));
                msg.Subject = "Test";
                msg.Body = new TextPart("");

                await _mq.EnqueueAsync(msg);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
