namespace Profakt.Intranet.Models
{
    public class BaseEntity
    {
        public Guid Id { get; set;  } = Guid.NewGuid();
    }
}
