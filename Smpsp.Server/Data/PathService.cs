namespace Smpsp.Server.Data
{
    public sealed class PathService : IDisposable
    {
        public const string BaseFolderName = "appdatas";
        public const string FilesFloderName = "files";
        public const string TempFloderName = "temp";
        public const string FFMpegFolderName = "ffmpeg";

        public string BasePath { get; private set; }
        public string FilesPath { get; private set; }
        public string TempPath { get; private set; }
        public string FFMpegPath { get; private set; }

        public PathService(string basePath)
        {
            BasePath = Path.Join(basePath, BaseFolderName);
            if (Directory.Exists(BasePath) == false)
                Directory.CreateDirectory(BasePath);

            FilesPath = Path.Join(BasePath, FilesFloderName);
            if (Directory.Exists(FilesPath) == false)
                Directory.CreateDirectory(FilesPath);

            TempPath = Path.Join(FilesPath, TempFloderName);
            if (Directory.Exists(TempPath) == false)
                Directory.CreateDirectory(TempPath);

            FFMpegPath = Path.Join(BasePath, FFMpegFolderName);
            if (Directory.Exists(FFMpegPath) == false)
                Directory.CreateDirectory(FFMpegPath);
        }

        public void Dispose()
        {
            var files = Directory.GetFiles(TempPath);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
