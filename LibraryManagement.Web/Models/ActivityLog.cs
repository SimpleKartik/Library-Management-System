using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Web.Models
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ActionType { get; set; } = string.Empty; // Create, Edit, Delete, Borrow, Return

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty; // Book, Student, Librarian, BorrowRecord

        public int? EntityId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty; // Which user did it

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Details { get; set; }
    }
}
