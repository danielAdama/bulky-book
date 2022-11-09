using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BulkyBookWeb.Models
{
	public class ChangesLog : BaseEntity
	{
        public long SyncId { get; set; }
        public string? UserId { get; set; }
        public long RowAffected { get; set; }
        public string? Changes { get; set; }
        public string? refCode { get; set; }
    }
}
