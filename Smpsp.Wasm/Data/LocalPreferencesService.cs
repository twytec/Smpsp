using Microsoft.JSInterop;

namespace Smpsp.Wasm.Data
{
    public class LocalPreferencesService(IJSRuntime _js)
    {
        public string? Token { get; set; }
        public User User { get; set; } = new();
        public long UnixTimestampExpirationDate { get; set; }

        private const string LanguageKey = "languageKey";
        private const string TokenKey = "tokenKey";
        private const string UserKey = "user";
        private const string UnixTimestampExpirationDateKey = "UnixTimestampExpirationDateKey";

        public async Task<string?> GetPreferencesAsync(string key)
        {
            return await _js.InvokeAsync<string?>("getStorage", key);
        }

        public async Task SetPreferencesAsync(string key, string value)
        {
            await _js.InvokeVoidAsync("setStorage", key, value);
        }

        public async Task DeletePreferencesAsync(string key)
        {
            await _js.InvokeVoidAsync("deleteStorage", key);
        }

        public async Task LoadPreferencesAsync()
        {
            Token = await GetPreferencesAsync(TokenKey);

            if (await GetPreferencesAsync(UserKey) is string json && Helpers.Json.TryGetModel<User>(json, out var u))
            {
                User = u;
            }

            if (await GetPreferencesAsync(UnixTimestampExpirationDateKey) is string s && long.TryParse(s, out var ticks))
            {
                UnixTimestampExpirationDate = ticks;
            }
        }

        public async Task SavePreferencesAsync()
        {
            if (Token is not null)
                await SetPreferencesAsync(TokenKey, Token);

            if (User is not null)
                await SetPreferencesAsync(UserKey, Helpers.Json.GetJson(User));

            await SetPreferencesAsync(UnixTimestampExpirationDateKey, UnixTimestampExpirationDate.ToString());
        }

        public async Task DeleteAllPreferencesAsync()
        {
            if (Token is not null)
                await DeletePreferencesAsync(TokenKey);

            if (User is not null)
                await DeletePreferencesAsync(UserKey);

            await DeletePreferencesAsync(UnixTimestampExpirationDateKey);

            Token = null;
            User = new();
            UnixTimestampExpirationDate = 0;
        }
    }
}
