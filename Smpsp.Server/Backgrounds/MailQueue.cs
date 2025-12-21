using MimeKit;
using System.Threading.Channels;

namespace Smpsp.Server.Backgrounds
{
    public class MailQueue
    {
        private readonly Channel<MimeMessage> _channel = Channel.CreateUnbounded<MimeMessage>();

        public int Count()
        {
            if (_channel.Reader.CanCount)
            {
                return _channel.Reader.Count;
            }

            return 0;
        }

        public async ValueTask EnqueueAsync(MimeMessage msg)
        {
            await _channel.Writer.WriteAsync(msg);
        }

        public async ValueTask<MimeMessage> DequeueAsync(CancellationToken ct)
        {
            var msg = await _channel.Reader.ReadAsync(ct);
            return msg;
        }
    }
}
