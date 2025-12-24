namespace Smpsp.Server.Data
{
    public class TranslationService
    {
        public Translation DefaultTranslations { get; set; }
        private readonly Dictionary<string, Translation> _trans = [];

        public TranslationService(string path, MySettings opt)
        {
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    var json = File.ReadAllText(file);
                    if (Helpers.Json.TryGetModel<Translation>(json, out var data))
                    {
                        _trans[data.LanguageCode] = data;
                    }
                }

                DefaultTranslations = GetTranslations(opt.LanguageCode);
            }
            else
                DefaultTranslations = new();
        }

        public string[] GetSupportedLanguages() => _trans.Keys.ToArray();

        public Translation GetTranslations(string code)
        {
            if (_trans.Count == 0)
                return new();
            else if (_trans.TryGetValue(code, out var data))
                return data;
            else if (code.Split('-') is string[] s && _trans.Values.FirstOrDefault(x => x.LanguageCode.StartsWith(s[0])) is Translation t)
                return t;
            else if (_trans.TryGetValue("en-us", out data))
                return data;
            else
                return _trans.First().Value;
        }
    }
}
