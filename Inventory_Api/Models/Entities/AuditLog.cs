using System.ComponentModel.DataAnnotations;

namespace Inventory_Api.Models.Entities
{
    public class AuditLog : BaseEntity
    {
        [Required]
        public string TableName { get; set; }

        [Required]
        public string Action { get; set; } // CREATE, UPDATE, DELETE

        public string? RecordId { get; set; }

        [Required]
        public string Description { get; set; }

        public string? OldValues { get; set; }
        public string? NewValues { get; set; }

        // Foreign Key
        public int UserId { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }
    }
}