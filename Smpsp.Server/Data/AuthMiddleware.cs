namespace Smpsp.Server.Data
{
    public class AuthMiddleware(RequestDelegate _next, MySettingsService _mss, UserService _us, TranslationService _ts)
    {
        public async Task InvokeAsync(HttpContext httpContext, UserAuthStateService authService)
        {
            if (httpContext.User.Identity is not null && httpContext.User.Identity.IsAuthenticated)
            {
                if (httpContext.User.Identity.Name == _mss.Settings.AdminName)
                {
                    authService.User = new() { EMail = _mss.Settings.AdminName };
                    authService.I18n = _ts.DefaultTranslations;
                }
                else
                {
                    authService.User = _us.GetUserBasedIdentity(httpContext.User.Identity);
                    if (authService.User != null)
                        authService.I18n = _ts.GetTranslations(authService.User.LanguageCode);
                }
            }

            if (authService.User is null)
                authService.I18n = _ts.DefaultTranslations;

            await _next(httpContext);
        }
    }
}
