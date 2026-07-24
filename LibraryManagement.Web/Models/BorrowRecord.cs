using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Web.Models
{
    public class BorrowRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }
        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        [Required]
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BorrowDate { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Borrowed"; // 'Borrowed', 'Returned', 'Overdue'
    }
}
