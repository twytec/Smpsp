namespace Smpsp
{
    public class SignInCodeRequest
    {
        public string AntiforgeryToken { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
