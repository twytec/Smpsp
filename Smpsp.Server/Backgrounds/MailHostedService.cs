
using Smpsp.Server.Data;

namespace Smpsp.Server.Backgrounds
{
    public class MailHostedService(ILogger<MailHostedService> _log, MailQueue _mq, MySettingsService _mss) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            MailKit.Net.Smtp.SmtpClient? smtp = null;

            while (stoppingToken.IsCancellationRequested == false)
            {
                var mail = await _mq.DequeueAsync(stoppingToken);

                if (smtp is null)
                {
                    smtp = new MailKit.Net.Smtp.SmtpClient();
                    await smtp.ConnectAsync(_mss.Settings.SmtpServer, _mss.Settings.SmtpPort, cancellationToken: stoppingToken);
                    await smtp.AuthenticateAsync(_mss.Settings.SmtpUsername, _mss.Settings.SmtpPassword, stoppingToken);
                }

                try
                {
                    await smtp.SendAsync(mail, stoppingToken);

                    if (_mq.Count() == 0)
                    {
                        await smtp.DisconnectAsync(true, stoppingToken);
                        smtp.Dispose();
                        smtp = null;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is not OperationCanceledException)
                    {
                        _log.LogError(ex, nameof(ExecuteAsync));
                    }
                }
            }

            if (smtp is not null)
            {
                await smtp.DisconnectAsync(true, stoppingToken);
                smtp.Dispose();
                smtp = null;
            }
        }
    }
}
