using LibraryManagement.Web.Data;
using LibraryManagement.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly LibraryDbContext _context;

        public DashboardController(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Librarian"))
            {
                var viewModel = new AdminDashboardViewModel
                {
                    TotalBooks = await _context.Books.SumAsync(b => b.TotalCopies),
                    TotalStudents = await _context.Students.CountAsync(),
                    TotalLibrarians = await _context.Librarians.CountAsync(),
                    BorrowedBooks = await _context.BorrowRecords.CountAsync(br => br.Status == "Borrowed"),
                    AvailableBooks = await _context.Books.SumAsync(b => (int?)b.AvailableCopies) ?? 0,
                    ReturnedBooks = await _context.BorrowRecords.CountAsync(br => br.Status == "Returned"),
                    
                    RecentlyAddedBooks = await _context.Books
                        .OrderByDescending(b => b.Id)
                        .Take(5)
                        .ToListAsync(),
                        
                    RecentBorrowActivities = await _context.BorrowRecords
                        .Include(br => br.Book)
                        .Include(br => br.Student)
                        .OrderByDescending(br => br.BorrowDate)
                        .Take(5)
                        .ToListAsync()
                };

                // Chart Data: Borrow Analytics (Last 7 Days)
                var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-i)).Reverse().ToList();
                var borrowCounts = new List<int>();
                
                foreach (var date in last7Days)
                {
                    var count = await _context.BorrowRecords
                        .Where(r => r.BorrowDate.Date == date)
                        .CountAsync();
                    borrowCounts.Add(count);
                }

                ViewBag.ChartLabels = string.Join(",", last7Days.Select(d => $"'{d.ToString("MMM dd")}'"));
                ViewBag.ChartData = string.Join(",", borrowCounts);

                // Chart Data: Book Status
                var availableCopies = await _context.Books.SumAsync(b => b.AvailableCopies);
                var borrowedCopies = await _context.BorrowRecords.CountAsync(r => r.Status == "Borrowed");
                
                ViewBag.AvailableCopies = availableCopies;
                ViewBag.BorrowedCopies = borrowedCopies;

                return View("AdminDashboard", viewModel);
            }
            
            // Render basic Student dashboard if not an Admin
            return View();
        }
    }
}
