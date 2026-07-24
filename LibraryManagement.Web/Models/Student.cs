using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Web.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number format")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Registration Number is required")]
        [StringLength(20)]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        // Foreign Key
        public int LoginId { get; set; }
        [ForeignKey("LoginId")]
        public Login? Login { get; set; }

        // Navigation property
        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    }
}
