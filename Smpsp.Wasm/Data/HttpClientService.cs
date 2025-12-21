using MudBlazor;
using System.Net.Http.Headers;

namespace Smpsp.Wasm.Data
{
    public class HttpClientService(IHttpClientFactory _hcf, IDialogService _dlg, LocalPreferencesService _lps, TranslationService _ts)
    {
        public async Task<bool> CheckAuthStateAsync()
        {
            if (string.IsNullOrEmpty(_lps.Token) || DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= _lps.UnixTimestampExpirationDate)
            {
                return false;
            }

            var hc = GetHttpClient();
            using var res = await hc.GetAsync("api/PingSignIn");
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }

            await _lps.DeleteAllPreferencesAsync();
            return false;
        }

        public async Task<HttpClient> GetHttpClientAsync()
        {
            DialogOptions opt = new() { CloseButton = false };

            while (string.IsNullOrEmpty(_lps.Token) || DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= _lps.UnixTimestampExpirationDate)
            {
                var dRef = await _dlg.ShowAsync<Dialogs.SignInDialog>(_ts.I18n.SigIn, opt);
                var dRes = await dRef.Result;
                if (dRes is not null && dRes.Data is SignInCodeReply rep)
                {
                    _lps.Token = rep.Token;
                    _lps.UnixTimestampExpirationDate = rep.UnixTimestampExpirationDate;
                    _lps.User = rep.User;
                    await _ts.SetTranslationsAsync(rep.User.LanguageCode);
                    await _lps.SavePreferencesAsync();
                }
            }

            return GetHttpClient();
        }

        private HttpClient GetHttpClient()
        {
            var hc = _hcf.CreateClient(HttpClientNames.Api);
            hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _lps.Token);
            return hc;
        }
    }
}
