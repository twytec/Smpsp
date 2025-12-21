using System.Net.Http.Json;

namespace Smpsp.Wasm.Data
{
    public class TranslationService(IHttpClientFactory _hcf)
    {
        public Translation I18n { get; set; } = new();
        public List<string> Languages { get; set; } = [];

        public async Task InitializeAsync()
        {
            var hc = _hcf.CreateClient(HttpClientNames.Api);
            if (await hc.GetFromJsonAsync<IEnumerable<string>>($"api/translation") is IEnumerable<string> l)
                Languages.AddRange(l);

            await SetTranslationsAsync(System.Globalization.CultureInfo.CurrentCulture.Name);
        }

        public async Task SetTranslationsAsync(string code)
        {
            var hc = _hcf.CreateClient(HttpClientNames.Api);
            var i18n = await hc.GetFromJsonAsync<Translation>($"api/translation/{code}");
            if (i18n is not null)
            {
                I18n = i18n;
            }
        }
    }
}
