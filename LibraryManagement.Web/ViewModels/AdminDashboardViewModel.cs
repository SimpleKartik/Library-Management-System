using LibraryManagement.Web.Models;

namespace LibraryManagement.Web.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalStudents { get; set; }
        public int TotalLibrarians { get; set; }
        public int BorrowedBooks { get; set; }
        public int AvailableBooks { get; set; }
        public int ReturnedBooks { get; set; }

        public List<Book> RecentlyAddedBooks { get; set; } = new List<Book>();
        public List<BorrowRecord> RecentBorrowActivities { get; set; } = new List<BorrowRecord>();
    }
}
