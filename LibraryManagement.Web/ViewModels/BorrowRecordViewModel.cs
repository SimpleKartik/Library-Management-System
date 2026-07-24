using LibraryManagement.Web.Models;

namespace LibraryManagement.Web.ViewModels
{
    public class BorrowRecordViewModel
    {
        public int Id { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsOverdue => Status == "Borrowed" && DateTime.Now > DueDate;
    }
}
