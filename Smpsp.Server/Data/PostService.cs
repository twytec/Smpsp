using Microsoft.EntityFrameworkCore;
using Smpsp.Server.Backgrounds;
using System.Collections.Concurrent;

namespace Smpsp.Server.Data
{
    public sealed class PostService
    {
        public IEnumerable<Post> GetPosts() => _posts.Values;
        public Post? GetPostById(string id)
        {
            _posts.TryGetValue(id, out var post);
            return post;
        }

        private readonly IDbContextFactory<AppDbContext> _dbF;
        private readonly PathService _ps;
        private readonly MySettingsService _mss;
        private readonly TranslationService _ts;
        private readonly CompletePostQueue _cpq;
        private readonly ILogger<PostService> _log;

        private readonly ConcurrentDictionary<string, Post> _posts = [];

        public PostService(IDbContextFactory<AppDbContext> dbF, PathService ps, MySettingsService mss, TranslationService ts, CompletePostQueue cpq, ILogger<PostService> log)
        {
            _dbF = dbF;
            _ps = ps;
            _mss = mss;
            _ts = ts;
            _cpq = cpq;
            _log = log;

            using var db = _dbF.CreateDbContext();
            var rp = db.Records.Where(x => x.RecordType == DataRecordType.Post);

            if (rp.Any())
            {
                foreach (var r in rp)
                {
                    if (Helpers.Json.TryGetModel<Post>(r.Data, out var p))
                    {
                        _posts[p.Id] = p;
                    }
                }
            }
        }

        public async Task CleanupPostsAsync(CancellationToken ct)
        {
            long delTicks = 0;
            if (_mss.Settings.DeletePostAfterDays > 0)
                delTicks = DateTimeOffset.UtcNow.AddDays(-_mss.Settings.DeletePostAfterDays).ToUnixTimeSeconds();

            var ticks = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            foreach (var item in _posts.ToList())
            {
                if (ct.IsCancellationRequested)
                    break;

                try
                {
                    if (item.Value.CreatedUnixTimestamp < delTicks)
                    {
                        _posts.TryRemove(item);
                        await DeletePostAsync(item.Key);
                    }
                    else if (item.Value.EndOfVoting < ticks)
                    {
                        if (item.Value.Vetoes.Count > 0 || item.Value.Votings.Count == 0)
                            item.Value.Status = PostStatus.NotSelected;
                        else
                        {
                            var like = item.Value.Votings.Where(x => x.Like == true).Count();
                            var notLike = item.Value.Votings.Count - like;
                            if (like > notLike)
                                item.Value.Status = PostStatus.Selected;
                            else
                                item.Value.Status = PostStatus.NotSelected;
                        }

                        await UpdatePostAsync(item.Value);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public async Task AddPostAsync(Post post)
        {
            try
            {
                post.CreatedUnixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                post.Status = PostStatus.Voting;
                post.CreationStatus = CreationStatus.CreateZip;
                post.Votings.Clear();
                post.Vetoes.Clear();

                if (post.Medias.Count > 0)
                {
                    foreach (var item in post.Medias)
                    {
                        if (item.File.StartsWith(PostMedia.ConvertTag))
                        {
                            item.MustBeConverted = true;
                            post.CreationStatus = CreationStatus.Convert;
                        }

                        var temp = Path.Join(_ps.TempPath, item.File);
                        File.Move(temp, Path.Join(_ps.FilesPath, item.File));
                    }
                }

                using var db = await _dbF.CreateDbContextAsync();
                var record = new DataRecord
                {
                    Id = post.Id,
                    RecordType = DataRecordType.Post,
                    Data = Helpers.Json.GetJson(post),
                    UnixTimestamp = post.CreatedUnixTimestamp,
                    UserId = post.UserId
                };
                db.Records.Add(record);
                await db.SaveChangesAsync();
                _posts[post.Id] = post;
                await _cpq.EnqueueAsync(post);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, nameof(AddPostAsync));
                throw new Exception(_ts.DefaultTranslations.UnknownError);
            }
        }

        public async Task UpdatePostAsync(Post post)
        {
            try
            {
                if (_posts.ContainsKey(post.Id))
                {
                    using var db = await _dbF.CreateDbContextAsync();
                    var record = await db.Records.FirstOrDefaultAsync(x => x.Id == post.Id);
                    if (record is not null)
                    {
                        record.RecordType = DataRecordType.Post;
                        record.UpdateData(post);
                        await db.SaveChangesAsync();
                        _posts[post.Id] = post;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, nameof(UpdatePostAsync));
                throw new Exception(_ts.DefaultTranslations.UnknownError);
            }
        }

        public async Task DeletePostAsync(string postId)
        {
            try
            {
                if (_posts.TryRemove(postId, out var p))
                {
                    using var db = await _dbF.CreateDbContextAsync();
                    var record = await db.Records.FirstOrDefaultAsync(x => x.Id == postId && x.RecordType == DataRecordType.Post);
                    if (record is not null)
                    {
                        db.Records.Remove(record);
                        await db.SaveChangesAsync();
                    }

                    try
                    {
                        foreach (var m in p.Medias)
                        {
                            var f = Path.Join(_ps.FilesPath, m.File);
                            if (File.Exists(f))
                                File.Delete(f);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, nameof(DeletePostAsync));
                throw new Exception(_ts.DefaultTranslations.UnknownError);
            }
        }
    }
}
