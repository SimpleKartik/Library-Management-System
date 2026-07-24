using LibraryManagement.Web.Data;
using LibraryManagement.Web.Helpers;
using LibraryManagement.Web.Models;
using LibraryManagement.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Web.Controllers
{
    [Authorize]
    public class BorrowRecordsController : Controller
    {
        private readonly LibraryDbContext _context;

        public BorrowRecordsController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: BorrowRecords
        public async Task<IActionResult> Index(string sortOrder, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "date_asc" : "";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "status_desc" : "Status";
            ViewData["CurrentFilter"] = searchString;

            var records = _context.BorrowRecords
                .Include(r => r.Book)
                .Include(r => r.Student)
                .AsQueryable();

            // Filter for student if not a librarian
            if (User.IsInRole("Student"))
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                if (userIdClaim != null)
                {
                    int userId = int.Parse(userIdClaim);
                    records = records.Where(r => r.Student != null && r.Student.LoginId == userId);
                }
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                records = records.Where(r => (r.Book != null && r.Book.Title.Contains(searchString))
                                          || (r.Student != null && r.Student.Name.Contains(searchString))
                                          || r.Status.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "date_asc":
                    records = records.OrderBy(r => r.BorrowDate);
                    break;
                case "Status":
                    records = records.OrderBy(r => r.Status);
                    break;
                case "status_desc":
                    records = records.OrderByDescending(r => r.Status);
                    break;
                default:
                    records = records.OrderByDescending(r => r.BorrowDate);
                    break;
            }

            var viewModels = records.Select(r => new BorrowRecordViewModel
            {
                Id = r.Id,
                BookTitle = r.Book != null ? r.Book.Title : "Unknown Book",
                StudentName = r.Student != null ? r.Student.Name : "Unknown Student",
                BorrowDate = r.BorrowDate,
                DueDate = r.DueDate,
                ReturnDate = r.ReturnDate,
                Status = r.Status
            });

            int pageSize = 10;
            return View(await PaginatedList<BorrowRecordViewModel>.CreateAsync(viewModels.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Borrow(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) 
            {
                TempData["ErrorMessage"] = "Book not found.";
                return RedirectToAction("Index", "Books");
            }

            if (book.AvailableCopies <= 0)
            {
                TempData["ErrorMessage"] = "No copies available to borrow.";
                return RedirectToAction("Index", "Books");
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (userIdClaim == null) return Unauthorized();
            
            int userId = int.Parse(userIdClaim);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.LoginId == userId);
            if (student == null) return Unauthorized();

            // Prevent duplicate active borrowing
            var existingBorrow = await _context.BorrowRecords
                .AnyAsync(r => r.BookId == bookId && r.StudentId == student.Id && r.Status == "Borrowed");

            if (existingBorrow)
            {
                TempData["ErrorMessage"] = "You have already borrowed this book and have not returned it.";
                return RedirectToAction("Index", "Books");
            }

            // Create record
            var record = new BorrowRecord
            {
                BookId = book.Id,
                StudentId = student.Id,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                Status = "Borrowed"
            };

            book.AvailableCopies--;
            _context.BorrowRecords.Add(record);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"You have successfully borrowed '{book.Title}'. Due on {record.DueDate.ToShortDateString()}.";
            return RedirectToAction("Index"); // Redirect to history
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Return(int recordId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var student = await _context.Students.FirstOrDefaultAsync(s => s.LoginId == userId);
            if (student == null) return Unauthorized();

            var record = await _context.BorrowRecords
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == recordId && r.StudentId == student.Id);

            if (record == null)
            {
                TempData["ErrorMessage"] = "Borrow record not found.";
                return RedirectToAction(nameof(Index));
            }

            if (record.Status != "Borrowed")
            {
                TempData["ErrorMessage"] = "This book is already returned.";
                return RedirectToAction(nameof(Index));
            }

            record.ReturnDate = DateTime.Now;
            
            // Determine Late Status
            if (record.ReturnDate.Value.Date > record.DueDate.Date)
            {
                record.Status = "Returned (Late)";
            }
            else
            {
                record.Status = "Returned";
            }

            record.Book!.AvailableCopies++;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"You have successfully returned '{record.Book.Title}'.";
            return RedirectToAction(nameof(Index));
        }
    }
}
