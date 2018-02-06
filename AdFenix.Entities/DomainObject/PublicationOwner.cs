using System.ComponentModel.DataAnnotations;

namespace AdFenix.Entities.DomainObject
{
    public class PublicationOwner
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string HostUrl { get; set; }
    }
}
