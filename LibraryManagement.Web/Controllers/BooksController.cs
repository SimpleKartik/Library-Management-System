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
    public class BooksController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BooksController(LibraryDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Books
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["AuthorSortParm"] = sortOrder == "Author" ? "author_desc" : "Author";
            ViewData["CopiesSortParm"] = sortOrder == "Copies" ? "copies_desc" : "Copies";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var books = from b in _context.Books select b;

            // Search
            if (!String.IsNullOrEmpty(searchString))
            {
                books = books.Where(s => s.Title.Contains(searchString)
                                       || s.Author.Contains(searchString)
                                       || s.ISBN.Contains(searchString));
            }

            // Sorting
            switch (sortOrder)
            {
                case "title_desc":
                    books = books.OrderByDescending(b => b.Title);
                    break;
                case "Author":
                    books = books.OrderBy(b => b.Author);
                    break;
                case "author_desc":
                    books = books.OrderByDescending(b => b.Author);
                    break;
                case "Copies":
                    books = books.OrderBy(b => b.AvailableCopies);
                    break;
                case "copies_desc":
                    books = books.OrderByDescending(b => b.AvailableCopies);
                    break;
                default:
                    books = books.OrderBy(b => b.Title);
                    break;
            }

            int pageSize = 5;
            return View(await PaginatedList<Book>.CreateAsync(books.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.BorrowRecords)
                .ThenInclude(br => br.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null) return NotFound();

            return View(book);
        }

        // GET: Books/Create
        [Authorize(Roles = "Librarian")]
        public IActionResult Create()
        {
            return View(new BookFormViewModel());
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = ProcessUploadedFile(model);
                var book = new Book
                {
                    Title = model.Title,
                    ISBN = model.ISBN,
                    Author = model.Author,
                    Publisher = model.Publisher,
                    TotalCopies = model.TotalCopies,
                    AvailableCopies = model.TotalCopies, // Initial setup
                    ImageUrl = uniqueFileName
                };

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Books/Edit/5
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            var model = new BookFormViewModel
            {
                Id = book.Id,
                Title = book.Title,
                ISBN = book.ISBN,
                Author = book.Author,
                Publisher = book.Publisher,
                TotalCopies = book.TotalCopies,
                ExistingImageUrl = book.ImageUrl
            };
            return View(model);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Edit(int id, BookFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null) return NotFound();

                book.Title = model.Title;
                book.ISBN = model.ISBN;
                book.Author = model.Author;
                book.Publisher = model.Publisher;

                // Adjust Available copies based on TotalCopies changes
                int diff = model.TotalCopies - book.TotalCopies;
                book.TotalCopies = model.TotalCopies;
                book.AvailableCopies += diff;

                if (model.CoverImage != null)
                {
                    if (book.ImageUrl != null)
                    {
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "books", book.ImageUrl);
                        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                    }
                    book.ImageUrl = ProcessUploadedFile(model);
                }

                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                book.IsDeleted = true; // Soft Delete
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }

        private string ProcessUploadedFile(BookFormViewModel model)
        {
            string uniqueFileName = null;
            if (model.CoverImage != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "books");
                Directory.CreateDirectory(uploadsFolder);
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.CoverImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.CoverImage.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
    }
}
