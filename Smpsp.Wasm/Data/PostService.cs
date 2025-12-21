using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Net.Http.Json;

namespace Smpsp.Wasm.Data
{
    public class PostService(HttpClientService _hcs, TranslationService _ts, ISnackbar _snackbar)
    {
        public EventHandler<PostService>? StateHasChanged;

        public List<Post> Posts { get; set; } = [];
        public List<User> Users { get; set; } = [];

        public async Task LoadPostsAsync()
        {
            var hc = await _hcs.GetHttpClientAsync();
            var res = await hc.GetFromJsonAsync<IEnumerable<Post>>("api/post");
            if (res is not null)
            {
                Posts.AddRange(res.OrderByDescending(x => x.CreatedUnixTimestamp));
            }

            var res2 = await hc.GetFromJsonAsync<IEnumerable<User>>("api/user");
            if (res2 is not null)
            {
                Users.AddRange(res2);
            }
        }

        public async Task GetPostByIdAsync(string id)
        {
            var hc = await _hcs.GetHttpClientAsync();
            var res = await hc.GetFromJsonAsync<Post>($"api/post/{id}");
            if (res is not null)
            {
                Posts.Add(res);
                StateHasChanged?.Invoke(this, this);
            }
        }

        public async Task<string> UploadMediaAsync(IBrowserFile file, long maxFileSize, int maxBodySize)
        {
            var hc = await _hcs.GetHttpClientAsync();
            DataMessage msg = new() { Extension = Path.GetExtension(file.Name) };
            maxBodySize -= Helpers.Json.GetJson(msg).Length;

            using var s = file.OpenReadStream(maxFileSize);
            byte[] data = new byte[maxBodySize];
            int numBytesToRead = (int)s.Length;
            int numBytesRead = 0;

            while (numBytesToRead > 0)
            {
                int n = await s.ReadAsync(data);
                msg.DataAsBase64 = Convert.ToBase64String(data, 0, n);
                if (n < data.Length)
                    msg.EOF = true;

                using var res = await hc.PostAsJsonAsync("api/post/media", msg);
                if (msg.Id == string.Empty && res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    msg.Id = await res.Content.ReadAsStringAsync();
                    msg.Extension = string.Empty;
                }
                else if (msg.EOF == true && res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    msg.Id = await res.Content.ReadAsStringAsync();
                }
                else if (res.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(await res.Content.ReadAsStringAsync());
                }

                numBytesRead += n;
                numBytesToRead -= n;
            }

            return msg.Id;
        }

        public async Task AddPostAsync(Post p)
        {
            var hc = await _hcs.GetHttpClientAsync();
            using var res = await hc.PostAsJsonAsync("api/post", p);
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var json = await res.Content.ReadAsStringAsync();
                if (Helpers.Json.TryGetModel<Post>(json, out var np))
                {
                    if (np.CreationStatus == CreationStatus.CreateZip)
                    {
                        Posts.Insert(0, np);
                        StateHasChanged?.Invoke(this, this);
                    }
                    else
                    {
                        _snackbar.Add(_ts.I18n.PostCreatedButNeedsConverted, Severity.Info);
                    }
                }
            }
            else
            {
                throw new Exception(await res.Content.ReadAsStringAsync());
            }
        }

        public async Task DeletePostAsync(Post p)
        {
            var hc = await _hcs.GetHttpClientAsync();
            using var res = await hc.DeleteAsync($"api/post/{p.Id}");
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Posts.Remove(p);
                StateHasChanged?.Invoke(this, this);
            }
            else
            {
                throw new Exception(await res.Content.ReadAsStringAsync());
            }
        }

        public async Task<string?> VoteAsync(string postId, PostVoting vote)
        {
            var hc = await _hcs.GetHttpClientAsync();
            using var res = await hc.PatchAsJsonAsync($"api/post/voting/{postId}", vote);
            if (res is not null)
            {
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return null;
                }
                return await res.Content.ReadAsStringAsync();
            }

            return _ts.I18n.UnknownError;
        }

        public async Task<string?> VetoAsync(string postId, string text)
        {
            var hc = await _hcs.GetHttpClientAsync();
            using var res = await hc.PatchAsJsonAsync($"api/post/veto/{postId}", new PostVeto() { Text = text });
            if (res is not null)
            {
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return null;
                }
                return await res.Content.ReadAsStringAsync();
            }

            return _ts.I18n.UnknownError;
        }

        public async Task<string?> VetoDeleteAsync(string postId, string userId)
        {
            var hc = await _hcs.GetHttpClientAsync();
            using var res = await hc.DeleteAsync($"api/post/veto/{postId}+{userId}");
            if (res is not null)
            {
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return null;
                }
                return await res.Content.ReadAsStringAsync();
            }

            return _ts.I18n.UnknownError;
        }
    }
}
