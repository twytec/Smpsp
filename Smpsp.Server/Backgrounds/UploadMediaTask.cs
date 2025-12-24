using SkiaSharp;
using Smpsp.Server.Data;
using System.Collections.Concurrent;

namespace Smpsp.Server.Backgrounds
{
    public sealed class UploadMediaTask(PathService _ps, MySettingsService _mss) : IDisposable
    {
        private const int _deadLine = 5;
        
        private record StreamingData(Stream Stream, DataMessage Data, bool IsImage, long MaxSize)
        {
            public DateTime DeadLine { get; set; } = DateTime.UtcNow.AddMinutes(1);
        }
        private readonly ConcurrentDictionary<string, StreamingData> _datas = [];

        public async Task WriteStreamAsync(DataMessage msg, UserAuthStateService uass)
        {
            if (msg.Id == string.Empty || _datas.ContainsKey(msg.Id) == false)
            {
                msg.Extension = msg.Extension.ToLower();
                if (msg.Extension.StartsWith('.') == false)
                    msg.Extension = $".{msg.Extension}";

                bool isImage;
                long maxSize;
                if (_mss.Settings.SupportedImageExtension.Contains(msg.Extension))
                {
                    isImage = true;
                    maxSize = _mss.Settings.MaxAllowedImageSize;
                }
                else if (_mss.Settings.SupportedVideoExtension.Contains(msg.Extension))
                {
                    isImage = false;
                    maxSize = _mss.Settings.MaxAllowedVideoSize;
                }
                else
                {
                    throw new Exception(uass.I18n.FileNotSupported);
                }

                msg.Id = $"{Guid.NewGuid()}{msg.Extension}";
                var file = Path.Join(_ps.TempPath, msg.Id);
                var stream = File.Create(file);

                var data = new StreamingData(stream, msg, isImage, maxSize);
                _datas[msg.Id] = data;
            }

            if (_datas.TryGetValue(msg.Id, out var d))
            {
                if (d.Stream.CanWrite)
                {
                    if (msg.DataAsBase64 is not null)
                    {
                        byte[] buffer = Convert.FromBase64String(msg.DataAsBase64);
                        if (d.Stream.Length + buffer.Length > d.MaxSize)
                        {
                            _datas.TryRemove(msg.Id, out _);
                            await d.Stream.DisposeAsync();
                            throw new Exception(uass.I18n.FileTooLarge.Replace("{0}", d.MaxSize.ToString()));
                        }

                        await d.Stream.WriteAsync(buffer);
                    }

                    if (msg.EOF)
                    {
                        try
                        {
                            _datas.TryRemove(msg.Id, out _);
                            var file = Path.Join(_ps.TempPath, msg.Id);

                            if (d.Stream.Length == 0)
                            {
                                await d.Stream.DisposeAsync();
                                throw new Exception(uass.I18n.FileIsCorrupted);
                            }
                            else
                            {
                                d.Stream.Seek(0, SeekOrigin.Begin);
                            }

                            if (d.IsImage)
                            {
                                var codec = SKCodec.Create(d.Stream);
                                var extension = $".{codec.EncodedFormat.ToString().ToLower()}";
                                codec.Dispose();

                                if (_mss.Settings.ImageConvertToPng.Contains(extension))
                                {
                                    msg.Id = $"{PostMedia.ConvertTag}{msg.Id}";
                                    File.Move(file, Path.Join(_ps.TempPath, msg.Id));
                                }
                            }
                            else
                            {

                                bool convert = false;

                                if (_mss.Settings.VideoConvertToWebm.Contains(d.Data.Extension))
                                {
                                    convert = true;
                                }
                                else
                                {
                                    var mediaInfo = await FFMpegCore.FFProbe.AnalyseAsync(d.Stream);
                                    convert = true;

                                    foreach (var stream in mediaInfo.VideoStreams)
                                    {
                                        if (stream.CodecName == "h264" || stream.CodecName == "vp8" || stream.CodecName == "vp9")
                                        {
                                            convert = false;
                                            break;
                                        }
                                    }

                                    if (convert == false && mediaInfo.AudioStreams.Count > 0)
                                    {
                                        convert = true;

                                        foreach (var stream in mediaInfo.AudioStreams)
                                        {
                                            if (stream.CodecName == "aac" || stream.CodecName == "opus")
                                            {
                                                convert = false;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (convert == true)
                                {
                                    msg.Id = $"{PostMedia.ConvertTag}{msg.Id}";
                                    File.Move(file, Path.Join(_ps.TempPath, msg.Id));
                                }
                                await d.Stream.FlushAsync();
                                await d.Stream.DisposeAsync();
                            }
                        }
                        catch (Exception)
                        {
                            await d.Stream.DisposeAsync();
                            throw new Exception(uass.I18n.FileIsCorrupted);
                        }
                    }
                    else
                    {
                        d.DeadLine = DateTime.UtcNow.AddMinutes(_deadLine);
                    }
                }
                else
                {
                    _datas.TryRemove(msg.Id, out _);
                    throw new Exception(uass.I18n.UnknownError);
                }
            }
            else
            {
                throw new Exception(uass.I18n.NotFound);
            }
        }

        public async Task CleanupStreamsAsync(CancellationToken ct)
        {
            if (_datas.IsEmpty == false)
            {
                foreach (var item in _datas.ToArray())
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    if (DateTime.UtcNow > item.Value.DeadLine)
                    {
                        if (_datas.TryRemove(item))
                        {
                            try
                            {
                                await item.Value.Stream.DisposeAsync();
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_datas.IsEmpty == false)
            {
                foreach (var item in _datas)
                {
                    try
                    {
                        item.Value.Stream.Close();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
