using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Smpsp.Wasm.Dialogs
{
    public partial class CreatePostDialog(Data.TranslationService _ts, Data.PostService _ps, ISnackbar _sb)
    {
        [CascadingParameter]
        private IMudDialogInstance? MudDialog { get; set; }

        [Parameter]
        public PostSettingsReply? PostSettings { get; set; }

        private bool _busy = false;

        private readonly Post _post = new();
        private IEnumerable<string>? _hashtags;
        private IReadOnlyCollection<string> _selectedHashtags = [];

        private string _mediaImageFormats = string.Empty;
        private string _mediaVideoFormats = string.Empty;

        private int _maxImageSize = 0;
        private int _maxVideoSize = 0;

        private int _votingHours = 48;

        private int _carouselSelectedIndex = 0;

        private bool _showUploadFile = true;
        private bool _showText = false;

        protected override void OnParametersSet()
        {
            if (PostSettings is not null)
            {
                if (PostSettings.SupportedImageExtension.Length > 0)
                    _mediaImageFormats = string.Join(", ", PostSettings.SupportedImageExtension);

                if (PostSettings.SupportedVideoExtension.Length > 0)
                    _mediaVideoFormats = string.Join(", ", PostSettings.SupportedVideoExtension);

                _maxImageSize = PostSettings.MaxAllowedImageSize;
                _maxVideoSize = PostSettings.MaxAllowedVideoSize;

                if (_mediaImageFormats == string.Empty && _mediaVideoFormats == string.Empty)
                {
                    _showUploadFile = false;
                    _showText = true;
                }

                _hashtags = PostSettings.Hashtags;
                _votingHours = PostSettings.DefaultVotingPeriodInHours;
            }
        }

        private void Next()
        {
            _showUploadFile = false;
            _showText = true;
        }

        private void Back()
        {
            _showUploadFile = true;
            _showText = false;
        }

        #region Upload media

        private async Task UploadImageFile(IBrowserFile file)
        {
            if (file.Size > _maxImageSize)
            {
                _sb.Add(_ts.I18n.FileTooLarge.Replace("{0}", _maxImageSize.ToString()), Severity.Error);
                return;
            }

            await UploadAsync(file, _maxImageSize, true);
        }

        private async Task UploadVideoFile(IBrowserFile file)
        {
            if (file.Size > _maxVideoSize)
            {
                _sb.Add(_ts.I18n.FileTooLarge.Replace("{0}", _maxVideoSize.ToString()), Severity.Error);
                return;
            }

            await UploadAsync(file, _maxVideoSize, false);
        }

        private async Task UploadAsync(IBrowserFile file, int maxFileSize, bool isImage)
        {
            if (PostSettings is null)
                return;

            _busy = true;
            StateHasChanged();

            try
            {
                var id = await _ps.UploadMediaAsync(file, maxFileSize, PostSettings.MaxRequestBodySize);
                _post.Medias.Add(new()
                {
                    File = id,
                    Extension = Path.GetExtension(file.Name),
                    MustBeConverted = id.StartsWith(PostMedia.ConvertTag),
                    IsImage = isImage
                });
            }
            catch (Exception ex)
            {
                _sb.Add(ex.Message, Severity.Error);
            }

            _busy = false;
        }

        private void CarouselSelectedIndecChanged(int index)
        {
            _carouselSelectedIndex = index;
            StateHasChanged();
        }

        #endregion

        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(_post.Text) && _post.Medias.Count == 0)
            {
                _sb.Add(_ts.I18n.TextIsRequired, Severity.Error);
                return;
            }

            _busy = true;
            StateHasChanged();

            try
            {
                _post.Hashtags = _selectedHashtags.ToArray();
                _post.EndOfVoting = DateTimeOffset.UtcNow.AddHours(_votingHours).ToUnixTimeSeconds();
                await _ps.AddPostAsync(_post);
                MudDialog?.Close();
            }
            catch (Exception ex)
            {
                _sb.Add(ex.Message, Severity.Error);
            }

            _busy = false;
        }
    }
}
