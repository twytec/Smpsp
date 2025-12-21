
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using Smpsp.Server.Data;
using System.IO.Compression;
using System.Text;

namespace Smpsp.Server.Backgrounds
{
    public class CompletePostHostedService(ILogger<CompletePostHostedService> _log, CompletePostQueue _pcq, PathService _path, PostService _ps) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var p in _ps.GetPosts())
            {
                if (p.CreationStatus != CreationStatus.Complete)
                {
                    await _pcq.EnqueueAsync(p);
                }
            }

            while (stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    var post = await _pcq.DequeueAsync(stoppingToken);

                    foreach (var item in post.Medias)
                    {
                        if (item.MustBeConverted)
                        {
                            string nf = item.File.Replace(PostMedia.ConvertTag, "");
                            var inputFile = Path.Join(_path.FilesPath, item.File);
                            var outputFile = Path.Join(_path.FilesPath, nf);

                            if (item.IsImage == false)
                            {
                                await FFMpegCore.FFMpegArguments
                                    .FromFileInput(inputFile)
                                    .OutputToFile(outputFile, true, opt => opt
                                        .WithVideoCodec("vp9")
                                        .ForceFormat("webm"))
                                    .ProcessAsynchronously();

                                item.Extension = ".webm";
                            }
                            else
                            {
                                var bitmap = SKBitmap.Decode(inputFile);
                                var fs = File.OpenWrite(outputFile);
                                bitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(fs);

                                bitmap.Dispose();
                                await fs.FlushAsync(stoppingToken);
                                await fs.DisposeAsync();

                                item.Extension = ".png";
                            }

                            item.File = nf;
                            item.MustBeConverted = false;
                        }
                    }

                    await CreateZipAsync(post);

                    post.CreationStatus = CreationStatus.Complete;
                    await _ps.UpdatePostAsync(post);
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

        private async Task CreateZipAsync(Post p)
        {
            StringBuilder sb = new();

            if (string.IsNullOrEmpty(p.Text) == false)
            {
                sb.Append(p.Text);
                sb.Append("\r\n");
                sb.Append("\r\n");
            }

            if (p.Hashtags.Length > 0)
            {
                sb.Append(string.Join(" ", p.Hashtags));
                sb.Append("\r\n");
                sb.Append("\r\n");
            }

            var file = Path.Join(_path.FilesPath, $"{p.Id}.zip");
            var zip = await ZipFile.OpenAsync(file, ZipArchiveMode.Create);
            int i = 0;
            foreach (var item in p.Medias)
            {
                var mf = Path.Join(_path.FilesPath, item.File);
                if (File.Exists(mf))
                {
                    i++;
                    var name = $"media{i}{Path.GetExtension(item.File)}";
                    await zip.CreateEntryFromFileAsync(mf, name);

                    if (string.IsNullOrEmpty(item.ContentAlt) == false)
                    {
                        sb.Append(name);
                        sb.Append("\r\n");
                        sb.Append(item.ContentAlt);
                        sb.Append("\r\n");
                        sb.Append("\r\n");
                    }
                }
            }

            if (sb.Length > 0)
            {
                var txt = Path.Join(_path.TempPath, $"{p.Id}.txt");
                await File.WriteAllTextAsync(txt, sb.ToString());
                await zip.CreateEntryFromFileAsync(txt, "Text.txt");
            }

            await zip.DisposeAsync();
        }
    }
}
