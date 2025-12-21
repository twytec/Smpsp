using System.Threading.Channels;

namespace Smpsp.Server.Backgrounds
{
    public class CompletePostQueue
    {
        private readonly Channel<Post> _channel = Channel.CreateUnbounded<Post>();

        public async ValueTask EnqueueAsync(Post post)
        {
            await _channel.Writer.WriteAsync(post);
        }

        public async ValueTask<Post> DequeueAsync(CancellationToken ct)
        {
            var post = await _channel.Reader.ReadAsync(ct);
            return post;
        }
    }
}
