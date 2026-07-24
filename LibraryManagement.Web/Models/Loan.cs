using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Web.Models
{
    public class Loan
    {
        public int Id { get; set; }
        
        public int BookId { get; set; }
        public Book? Book { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Borrower Name")]
        public string BorrowerName { get; set; } = string.Empty;
        
        public DateTime LoanDate { get; set; } = DateTime.Now;
        
        public DateTime? ReturnDate { get; set; }
        
        public bool IsReturned { get; set; }
    }
}
