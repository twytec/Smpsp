namespace Smpsp
{
    public class User : IEquatable<User?>
    {
        public string Id { get; set; } = string.Empty;
        public string EMail { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Active { get; set; }
        public string LanguageCode { get; set; } = string.Empty;
        public int VetoLevel { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as User);
        }

        public bool Equals(User? other)
        {
            return other is not null &&
                   Id == other.Id &&
                   EMail == other.EMail &&
                   Name == other.Name &&
                   Active == other.Active &&
                   LanguageCode == other.LanguageCode &&
                   VetoLevel == other.VetoLevel;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, EMail, Name, Active, LanguageCode);
        }

        public string GetName()
        {
            if (string.IsNullOrEmpty(Name))
                return EMail;

            return Name;
        }

        public void MergeFrom(User other)
        {
            EMail = other.EMail;
            Name = other.Name;
            Active = other.Active;
            LanguageCode = other.LanguageCode;
            VetoLevel = other.VetoLevel;
        }

        public User Clone()
        {
            return new()
            {
                Id = Id,
                EMail = EMail,
                Name = Name,
                Active = Active,
                LanguageCode = LanguageCode,
                VetoLevel = VetoLevel
            };
        }
    }
}
