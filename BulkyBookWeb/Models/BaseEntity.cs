using System.ComponentModel.DataAnnotations;

namespace BulkyBookWeb.Models
{
	public class BaseEntity
	{
        [Key]
        public long Id { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeUpdated { get; set; }
    }
}
