using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BulkyBookWeb.Models
{
	// New excel book upload to compare with a the library database
	public class SyncLog : BaseEntity
	{
        public long LibSyncId { get; set; }
        public string? UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Display Order must be between 1 and 100 only!")]
        public int DisplayOrder { get; set; }
        public string? Genre { get; set; }
        public string? ISBN { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public DateTime DateTimeNow { get; set; } = DateTime.Now;
    }
}
