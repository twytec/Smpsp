
using MudBlazor;
using Smpsp.Wasm.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Smpsp.Wasm.Pages
{
    public partial class Admin(IHttpClientFactory _hcf, IDialogService _dlg, TranslationService _ts)
    {
        private readonly List<User> _users = [];
        private string _searchString = string.Empty;
        private string? _token;
        private long _unixTimestampExpirationDate;
        private bool _busy = true;

        private readonly MySettings _mys = new();
        private readonly List<Edit> _mediaImageTypes = [];
        private readonly List<Edit> _mediaVideoTypes = [];
        private readonly List<Edit> _convertImageTypes = [];
        private readonly List<Edit> _convertVideoTypes = [];
        private readonly List<Edit> _hashtags = [];

        private class Edit(string item)
        {
            public string Item { get; set; } = item;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await _ts.InitializeAsync();
                await GetHttpClientAsync();
                await LoadDataAsync();

                _busy = false;
                StateHasChanged();
            }
        }

        private Func<User, bool> QuickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.EMail.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        private async ValueTask<HttpClient> GetHttpClientAsync()
        {
            DialogOptions opt = new() { CloseButton = false };
            while (string.IsNullOrEmpty(_token) || DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= _unixTimestampExpirationDate)
            {
                var dRef = await _dlg.ShowAsync<Dialogs.AdminSignInDialog>(_ts.I18n.SigIn, opt);
                var dRes = await dRef.Result;
                if (dRes is not null && dRes.Data is AdminSignInReply rep)
                {
                    _token = rep.Token;
                    _unixTimestampExpirationDate = rep.UnixTimestampExpirationDate;

                    break;
                }
            }

            var hc = _hcf.CreateClient(HttpClientNames.Api);
            hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            return hc;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var hc = await GetHttpClientAsync();
                var res = await hc.GetFromJsonAsync<IEnumerable<User>>("api/user");
                if (res is not null)
                {
                    _users.AddRange(res.OrderBy(x => x.Name));
                }

                var res2 = await hc.GetFromJsonAsync<MySettings>("api/settings");
                if (res2 is not null)
                {
                    _mys.MergeFrom(res2);

                    if (_mys.SupportedImageExtension.Length > 0)
                    {
                        foreach (var item in _mys.SupportedImageExtension)
                        {
                            _mediaImageTypes.Add(new(item));
                        }
                    }

                    if (_mys.SupportedVideoExtension.Length > 0)
                    {
                        foreach (var item in _mys.SupportedVideoExtension)
                        {
                            _mediaVideoTypes.Add(new(item));
                        }
                    }

                    if (_mys.ImageConvertToPng.Length > 0)
                    {
                        foreach (var item in _mys.ImageConvertToPng)
                        {
                            _convertImageTypes.Add(new(item));
                        }
                    }

                    if (_mys.VideoConvertToWebm.Length > 0)
                    {
                        foreach (var item in _mys.VideoConvertToWebm)
                        {
                            _convertVideoTypes.Add(new(item));
                        }
                    }

                    if (_mys.Hashtags.Length > 0)
                    {
                        foreach (var item in _mys.Hashtags)
                        {
                            _hashtags.Add(new(item));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _dlg.ShowMessageBox(_ts.I18n.Error, ex.Message);
            }

        }

        #region User

        private async Task AddUser()
        {
            try
            {
                User user = new() { Active = true, LanguageCode = _ts.I18n.LanguageCode };
                DialogParameters<Dialogs.AdminUserEditDialog> param = new()
                {
                    { x => x.User, user },
                };

                var dRef = await _dlg.ShowAsync<Dialogs.AdminUserEditDialog>(_ts.I18n.AddUser, param);
                var dRes = await dRef.Result;

                if (dRes is not null && dRes.Canceled == false)
                {
                    _busy = true;
                    StateHasChanged();

                    var hc = await GetHttpClientAsync();
                    var res = await hc.PostAsJsonAsync("api/user", user);
                    if (res.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var createdUser = await res.Content.ReadFromJsonAsync<User>();
                        if (createdUser is not null)
                        {
                            _users.Add(createdUser);
                        }
                    }
                    else
                    {
                        var errorMsg = await res.Content.ReadAsStringAsync();
                        await _dlg.ShowMessageBox(_ts.I18n.Error, errorMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                await _dlg.ShowMessageBox(_ts.I18n.Error, ex.Message);
            }
            finally
            {
                _busy = false;
            }
        }

        private async Task EditUser(User user)
        {
            try
            {
                DialogParameters<Dialogs.AdminUserEditDialog> param = new()
                {
                    { x => x.User, user }
                };

                var dRef = await _dlg.ShowAsync<Dialogs.AdminUserEditDialog>(_ts.I18n.AddUser, param);
                var dRes = await dRef.Result;

                if (dRes is not null && dRes.Canceled == false)
                {
                    _busy = true;
                    StateHasChanged();

                    var hc = await GetHttpClientAsync();
                    var res = await hc.PutAsJsonAsync("api/user", user);
                    if (res.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        var errorMsg = await res.Content.ReadAsStringAsync();
                        await _dlg.ShowMessageBox(_ts.I18n.Error, errorMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                await _dlg.ShowMessageBox(_ts.I18n.Error, ex.Message);
            }
            finally
            {
                _busy = false;
            }
        }

        private async Task DeleteUser(User user)
        {
            var ok = await _dlg.ShowMessageBox(_ts.I18n.ReallyDelete, user.GetName(), _ts.I18n.Yes, _ts.I18n.No);
            if (ok is true)
            {
                try
                {
                    _busy = true;
                    StateHasChanged();

                    var hc = await GetHttpClientAsync();
                    var res = await hc.DeleteAsync($"api/user/{user.Id}");
                    if (res.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        _users.Remove(user);
                    }
                    else
                    {
                        var errorMsg = await res.Content.ReadAsStringAsync();
                        await _dlg.ShowMessageBox(_ts.I18n.Error, errorMsg);
                    }
                }
                catch (Exception ex)
                {
                    await _dlg.ShowMessageBox(_ts.I18n.Error, ex.Message);
                }
                finally
                {
                    _busy = false;
                }
            }
        }

        #endregion

        #region Settings

        private async Task<bool> SaveSettingsAsync()
        {
            if (string.IsNullOrWhiteSpace(_mys.AdminName))
            {
                await _dlg.ShowMessageBox(_ts.I18n.Error, _ts.I18n.NameIsRequired);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_mys.AdminPassword))
            {
                await _dlg.ShowMessageBox(_ts.I18n.Error, _ts.I18n.PasswordIsRequired);
                return false;
            }

            try
            {
                _busy = true;
                StateHasChanged();

                _mys.SupportedImageExtension = _mediaImageTypes.Where(x => string.IsNullOrWhiteSpace(x.Item) == false).Select(x => x.Item).Distinct().ToArray();
                _mys.SupportedVideoExtension = _mediaVideoTypes.Where(x => string.IsNullOrWhiteSpace(x.Item) == false).Select(x => x.Item).Distinct().ToArray();

                _mys.ImageConvertToPng = _convertImageTypes.Where(x => string.IsNullOrWhiteSpace(x.Item) == false).Select(x => x.Item).Distinct().ToArray();
                _mys.VideoConvertToWebm = _convertVideoTypes.Where(x => string.IsNullOrWhiteSpace(x.Item) == false).Select(x => x.Item).Distinct().ToArray();

                List<string> hts = [];
                foreach (var item in _hashtags)
                {
                    string ht = item.Item;
                    if (item.Item.StartsWith('#') == false)
                        item.Item = $"#{item.Item}";

                    if (hts.Contains(item.Item) == false)
                        hts.Add(item.Item);
                }

                _mys.Hashtags = hts.OrderBy(x => x).ToArray();

                var hc = await GetHttpClientAsync();
                using var res = await hc.PutAsJsonAsync("api/settings", _mys);
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    var err = await res.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(err))
                    {
                        await _dlg.ShowMessageBox(_ts.I18n.Error, _ts.I18n.UnknownError);
                    }
                    else
                    {
                        await _dlg.ShowMessageBox(_ts.I18n.Error, err);
                    }
                }
            }
            catch (Exception ex)
            {
                await _dlg.ShowMessageBox(_ts.I18n.Error, ex.Message);
            }
            finally
            {
                _busy = false;
            }

            return false;
        }

        private async Task TestMail()
        {
            if (await SaveSettingsAsync() == false)
            {
                return;
            }

            try
            {
                _busy = true;
                StateHasChanged();

                var hc = await GetHttpClientAsync();

                using var res = await hc.GetAsync("api/settings/testmail");
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return;
                }
                else
                {
                    var err = await res.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(err))
                    {
                        await _dlg.ShowMessageBox(_ts.I18n.Error, _ts.I18n.UnknownError);
                    }
                    else
                    {
                        await _dlg.ShowMessageBox(_ts.I18n.Error, err);
                    }
                }
            }
            catch (Exception ex)
            {
                await _dlg.ShowMessageBox(_ts.I18n.Error, ex.Message);
            }
            finally
            {
                _busy = false;
            }
        }



        #endregion
    }
}
