using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryManagement.Web.ViewModels
{
    public class BookViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string ISBN { get; set; } = string.Empty;
        
        [Display(Name = "Published Date")]
        [DataType(DataType.Date)]
        public DateTime PublishedDate { get; set; }
        
        [Required]
        [Display(Name = "Author")]
        public int AuthorId { get; set; }
        
        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public bool IsAvailable { get; set; } = true;

        public IEnumerable<SelectListItem>? Authors { get; set; }
        public IEnumerable<SelectListItem>? Categories { get; set; }
    }
}
