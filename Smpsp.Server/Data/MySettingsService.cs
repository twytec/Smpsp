
namespace Smpsp.Server.Data
{
    public class MySettingsService
    {
        public MySettings Settings { get; set; }
        private readonly string _path;
        private readonly Lock _lock = new();

        public MySettingsService(PathService ps)
        {
            _path = Path.Join(ps.BasePath, "mysettings.json");

            if (File.Exists(_path))
            {
                try
                {
                    var json = File.ReadAllText(_path);
                    if (Helpers.Json.TryGetModel<MySettings>(json, out var data))
                    {
                        Settings = data;
                    }
                }
                catch (Exception)
                {
                }
            }

            if (Settings is null)
            {
                Settings = new()
                {
                    IssuerSigningKey = Guid.NewGuid().ToString("N")
                };
                SaveSettings();
            }
        }

        public void SaveSettings()
        {
            lock (_lock)
            {
                Settings.SupportedVideoExtension = ToLower(Settings.SupportedVideoExtension);
                Settings.SupportedVideoExtension = ToLower(Settings.SupportedVideoExtension);
                Settings.VideoConvertToWebm = ToLower(Settings.VideoConvertToWebm);
                Settings.ImageConvertToPng = ToLower(Settings.ImageConvertToPng);

                var json = Helpers.Json.GetJsonIndented(Settings);
                File.WriteAllText(_path, json);
            }
        }

        private static string[] ToLower(string[] strings)
        {
            List<string> lower = [];
            foreach (var item in strings)
            {
                lower.Add(item.ToLower());
            }
            return lower.ToArray();
        }
    }
}
