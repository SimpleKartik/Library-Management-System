using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Web.Models
{
    public class Login
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Student"; // 'Student' or 'Librarian'

        // Navigation properties
        public Student? Student { get; set; }
        public Librarian? Librarian { get; set; }
    }
}
