using Smpsp.Server.Data;

namespace Smpsp.Server.Backgrounds
{
    public sealed class CleanupHostedService(ILogger<CleanupHostedService> _log, PathService _path, PostService _ps, UploadMediaTask _umt, SignInCodeTask _sict) : BackgroundService, IDisposable
    {
        private const int CleanupIntervalMinutes = 5;

        private const int CleanupFilesIntervalHours = 24;
        private const string CleanupFileName = "cleanup.txt";
        private DateTime _lastCleanupFlies = DateTime.UtcNow.AddDays(-1);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cleanupFilePath = Path.Join(_path.FilesPath, CleanupFileName);
            if (File.Exists(cleanupFilePath))
            {
                var text = await File.ReadAllTextAsync(cleanupFilePath, stoppingToken);
                if (long.TryParse(text, out var t))
                {
                    _lastCleanupFlies = new(t);
                }
            }

            while (stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(CleanupIntervalMinutes), stoppingToken);

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    await _ps.CleanupPostsAsync(stoppingToken);
                    await _umt.CleanupStreamsAsync(stoppingToken);
                    _sict.Cleanup(stoppingToken);

                    var ts = DateTime.UtcNow - _lastCleanupFlies;
                    if (ts.TotalHours >= CleanupFilesIntervalHours)
                    {
                        await CleanupFilesAsync(stoppingToken);

                        _lastCleanupFlies = DateTime.UtcNow;
                        await File.WriteAllTextAsync(cleanupFilePath, _lastCleanupFlies.Ticks.ToString(), stoppingToken);
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
        }

        private async Task CleanupFilesAsync(CancellationToken ct)
        {
            DirectoryInfo temp = new(_path.TempPath);
            if (temp.Exists)
            {
                DateTime del = DateTime.UtcNow.AddDays(-1);

                foreach (var item in temp.GetFiles())
                {
                    if (item.CreationTimeUtc < del)
                    {
                        TryDeleteFile(item.FullName);
                    }
                }
            }

            DirectoryInfo files = new(_path.FilesPath);
            if (files.Exists)
            {
                var posts = _ps.GetPosts();
                foreach (var item in files.GetFiles())
                {
                    if (item.Extension == ".txt" || item.Extension == ".zip")
                    {
                        var n = Path.GetFileNameWithoutExtension(item.Name);
                        if (posts.Any(x => x.Id == n) == false)
                        {
                            TryDeleteFile(item.FullName);
                        }
                    }
                    else
                    {
                        var ava = from p in posts
                                  from m in p.Medias
                                  where m.File == item.Name
                                  select m;

                        if (ava.Any() == false)
                        {
                            TryDeleteFile(item.FullName);
                        }
                    }
                }
            }
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
