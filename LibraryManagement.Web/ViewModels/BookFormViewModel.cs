using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using LibraryManagement.Web.Models;

namespace LibraryManagement.Web.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN is required")]
        [RegularExpression(@"^(?:ISBN(?:-1[03])?:? )?(?=[0-9X]{10}$|(?=(?:[0-9]+[- ]){3})[- 0-9X]{13}$|97[89][0-9]{10}$|(?=(?:[0-9]+[- ]){4})[- 0-9]{17}$)(?:97[89][- ]?)?[0-9]{1,5}[- ]?[0-9]+[- ]?[0-9]+[- ]?[0-9X]$", ErrorMessage = "Invalid ISBN format")]
        [StringLength(20)]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Publisher { get; set; } = string.Empty;

        [Range(1, 1000, ErrorMessage = "Total copies must be between 1 and 1000")]
        public int TotalCopies { get; set; }

        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Cover Image")]
        public IFormFile? CoverImage { get; set; }
    }
}
