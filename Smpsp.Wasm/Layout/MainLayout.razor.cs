using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Smpsp.Wasm.Data;
using System.Net.Http.Json;

namespace Smpsp.Wasm.Layout
{
    public partial class MainLayout(NavigationManager _nm, IJSRuntime _js, LocalPreferencesService _lps, TranslationService _ts, HttpClientService _hcs, PostService _ps, IDialogService _dlg)
    {
        private bool _busy = true;
        private bool _isDarkMode = false;
        private MudTheme? _theme = null;
        private bool _loadView = false;
        private PostSettingsReply? _postSettingsReply;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _theme = new()
            {
                PaletteLight = _lightPalette,
                PaletteDark = _darkPalette,
                LayoutProperties = new LayoutProperties()
            };
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _isDarkMode = await _js.InvokeAsync<bool>("isDarkMode");
                await _ts.InitializeAsync();
                await _lps.LoadPreferencesAsync();

                if (await _hcs.CheckAuthStateAsync() == false)
                    await _hcs.GetHttpClientAsync();

                await _ps.LoadPostsAsync();
                _busy = false;
                _loadView = true;
                StateHasChanged();
            }
        }

        private void GoToHome() => _nm.NavigateTo("/");
        private void GoToSelected() => _nm.NavigateTo("/selected");
        private void GoToNotSelected() => _nm.NavigateTo("/notselected");
        private async Task CreatePost()
        {
            try
            {
                if (_postSettingsReply is null)
                {
                    _busy = true;
                    StateHasChanged();

                    var hc = await _hcs.GetHttpClientAsync();
                    _postSettingsReply = await hc.GetFromJsonAsync<PostSettingsReply>("api/post/settings");

                    _busy = false;
                }

                if (_postSettingsReply is not null)
                {
                    DialogParameters<Dialogs.CreatePostDialog> para = new()
                    {
                        { x => x.PostSettings, _postSettingsReply }
                    };

                    await _dlg.ShowAsync<Dialogs.CreatePostDialog>(_ts.I18n.CreatePost, para);
                }
            }
            catch (Exception ex)
            {
                await _dlg.ShowMessageBox(_ts.I18n.Error, ex.Message, _ts.I18n.Ok);
            }
        }

        private async Task PersonClick()
        {
            var u = _lps.User.Clone();

            DialogParameters<Dialogs.UserDialog> para = new()
            {
                { x => x.User, u }
            };

            var dRef = await _dlg.ShowAsync<Dialogs.UserDialog>(_ts.I18n.UserOptions, para);
            var res = await dRef.Result;
            if (res is not null && res.Canceled == false && u.Equals(_lps.User) == false)
            {
                _busy = true;
                StateHasChanged();

                try
                {
                    var hc = await _hcs.GetHttpClientAsync();
                    using var ok = await hc.PutAsJsonAsync("api/user", u);
                    if (ok.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        _lps.User.MergeFrom(u);
                        await _lps.SavePreferencesAsync();
                    }
                }
                catch (Exception ex)
                {
                    await _dlg.ShowMessageBox(_ts.I18n.Error, ex.Message, _ts.I18n.Ok);
                }

                _busy = false;
            }
        }

        private readonly PaletteLight _lightPalette = new()
        {
            Black = "#110e2d",
            AppbarText = "#424242",
            AppbarBackground = "rgba(255,255,255,0.8)",
            DrawerBackground = "#ffffff",
            GrayLight = "#e8e8e8",
            GrayLighter = "#f9f9f9",
        };

        private readonly PaletteDark _darkPalette = new()
        {
            Primary = "#7e6fff",
            Surface = "#1e1e2d",
            Background = "#1a1a27",
            BackgroundGray = "#151521",
            AppbarText = "#92929f",
            AppbarBackground = "rgba(26,26,39,0.8)",
            DrawerBackground = "#1a1a27",
            ActionDefault = "#74718e",
            ActionDisabled = "#9999994d",
            ActionDisabledBackground = "#605f6d4d",
            TextPrimary = "#b2b0bf",
            TextSecondary = "#92929f",
            TextDisabled = "#ffffff33",
            DrawerIcon = "#92929f",
            DrawerText = "#92929f",
            GrayLight = "#2a2833",
            GrayLighter = "#1e1e2d",
            Info = "#4a86ff",
            Success = "#3dcb6c",
            Warning = "#ffb545",
            Error = "#ff3f5f",
            LinesDefault = "#33323e",
            TableLines = "#33323e",
            Divider = "#292838",
            OverlayLight = "#1e1e2d80",
        };
    }
}