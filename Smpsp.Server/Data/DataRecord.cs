namespace Smpsp.Server.Data
{
    public class DataRecord : IEquatable<DataRecord?>
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public long UnixTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public DataRecordType RecordType { get; set; }
        public string Data { get; set; } = string.Empty;

        public override bool Equals(object? obj)
        {
            return Equals(obj as DataRecord);
        }

        public bool Equals(DataRecord? other)
        {
            return other is not null &&
                   Id == other.Id &&
                   UserId == other.UserId &&
                   UnixTimestamp == other.UnixTimestamp &&
                   RecordType == other.RecordType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, UserId, UnixTimestamp, RecordType);
        }

        public void MergeFrom(DataRecord record)
        {
            Id = record.Id;
            UserId = record.UserId;
            UnixTimestamp = record.UnixTimestamp;
            RecordType = record.RecordType;
            Data = record.Data;
        }

        public void UpdateData(object data)
        {
            Data = Helpers.Json.GetJson(data);
            UnixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    public enum DataRecordType
    {
        User = 0,
        Post = 1
    }
}
