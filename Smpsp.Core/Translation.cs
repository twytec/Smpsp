namespace Smpsp
{
    public class Translation
    {
        public string LanguageCode { get; set; } = string.Empty;

        //A
        public string Active { get; set; } = string.Empty;
        public string Add { get; set; } = string.Empty;
        public string AddUser { get; set; } = string.Empty;
        public string Administrator { get; set; } = string.Empty;
        public string AlternativeText { get; set; } = string.Empty;
        //B
        public string Back { get; set; } = string.Empty;
        //C
        public string Code { get; set; } = string.Empty;
        public string CodeIsRequired { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string ConvertFormatsToPng { get; set; } = string.Empty;
        public string ConvertFormatsToWebm { get; set; } = string.Empty;
        public string CreatePost { get; set; } = string.Empty;
        //D
        public string Delete { get; set; } = string.Empty;
        public string DeletePostAfterDays { get; set; } = string.Empty;
        //E
        public string Edit { get; set; } = string.Empty;
        public string EMail { get; set; } = string.Empty;
        public string EMailCode { get; set; } = string.Empty;
        public string EMailIsRequired { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        //F
        public string FileNotSupported { get; set; } = string.Empty;
        public string FileIsCorrupted { get; set; } = string.Empty;
        public string FileTooLarge { get; set; } = string.Empty;
        //G
        //H
        public string Hashtags { get; set; } = string.Empty;
        //I
        public string InvalidCode { get; set; } = string.Empty;
        public string InvalidNameOrPassword { get; set; } = string.Empty;
        public string InvalidOrInactiveUser { get; set; } = string.Empty;
        public string InvalidVetoId { get; set; } = string.Empty;
        //J
        //K
        //L
        public string Language { get; set; } = string.Empty;
        //M
        public string MaxAllowedImageSize { get; set; } = string.Empty;
        public string MaxAllowedVideoSize { get; set; } = string.Empty;
        public string MaxRequestBodySize { get; set; } = string.Empty;
        public string MustBeConverted { get; set; } = string.Empty;
        //N
        public string Name { get; set; } = string.Empty;
        public string NameIsRequired { get; set; } = string.Empty;
        public string Next { get; set; } = string.Empty;
        public string No { get; set; } = string.Empty;
        public string NotFound { get; set; } = string.Empty;
        public string NotSelected { get; set; } = string.Empty;
        //O
        public string Ok { get; set; } = string.Empty;
        public string OpenMenu { get; set; } = string.Empty;
        public string OverrideVeto { get; set; } = string.Empty;
        //P
        public string Password { get; set; } = string.Empty;
        public string PasswordIsRequired { get; set; } = string.Empty;
        public string PostCreatedButNeedsConverted { get; set; } = string.Empty;
        public string PostNotFound { get; set; } = string.Empty;
        //Q
        //R
        public string ReallyDelete { get; set; } = string.Empty;
        //S
        public string Save { get; set; } = string.Empty;
        public string Search { get; set; } = string.Empty;
        public string Selected { get; set; } = string.Empty;
        public string SelectImage { get; set; } = string.Empty;
        public string SelectVideo { get; set; } = string.Empty;
        public string Settings { get; set; } = string.Empty;
        public string SigIn { get; set; } = string.Empty;
        public string SignInExpirationDays { get; set; } = string.Empty;
        public string SignInCodeExpirationHours { get; set; } = string.Empty;
        public string SignInMailMessage { get; set; } = string.Empty;
        public string SignInMailSubject { get; set; } = string.Empty;
        public string SignOut { get; set; } = string.Empty;
        public string SupportedImageFormats { get; set; } = string.Empty;
        public string SupportedVideoFormats { get; set; } = string.Empty;
        //T
        public string Telephone { get; set; } = string.Empty;
        public string TestSmtp { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string TextIsRequired { get; set; } = string.Empty;
        //U
        public string UnknownError { get; set; } = string.Empty;
        public string Unknown { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string UserNotFound { get; set; } = string.Empty;
        public string UserOptions { get; set; } = string.Empty;
        public string UserAlreadyExists { get; set; } = string.Empty;
        public string UserDoesNotExist { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        //V
        public string Veto { get; set; } = string.Empty;
        public string VetoLevel { get; set; } = string.Empty;
        public string[] VetoNames { get; set; } = [];
        public string Voting { get; set; } = string.Empty;
        public string VotingComplete { get; set; } = string.Empty;
        public string VotingPeriodInHours { get; set; } = string.Empty;
        //W
        //X
        //Y
        public string Yes { get; set; } = string.Empty;
        //Z
    }

    public static class TranslationExtensions
    {
        public static string GetVetoName(this Translation translation, int vetoLevel)
        {
            if (vetoLevel < 0 || vetoLevel >= translation.VetoNames.Length)
                return string.Empty;

            return translation.VetoNames[vetoLevel];
        }
    }
}
