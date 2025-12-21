using Smpsp.Wasm.Data;

namespace Smpsp.Wasm.Pages
{
    public partial class Home(PostService _ps)
    {
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                _ps.StateHasChanged += PostChanged;
            }
        }

        private void PostChanged(object? sender, PostService e)
        {
            StateHasChanged();
        }
    }
}
