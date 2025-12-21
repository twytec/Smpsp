namespace Smpsp
{
    public class DataMessage
    {
        public string Id { get; set; } = string.Empty;
        public bool EOF { get; set; }
        public string Extension { get; set; } = string.Empty;
        public string DataAsBase64 { get; set; } = string.Empty;
    }
}
