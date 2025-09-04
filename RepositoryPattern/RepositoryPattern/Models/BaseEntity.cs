namespace RepositoryPattern.Models
{
    public class BaseEntity
    {
        public long Id { get; set; }

        public DateTimeOffset UpdatedWhen { get; set; }

        public DateTimeOffset CreatedWhen { get; set; }

        public uint Version { get; set; }
    }
}
