using LibraryManagement.Web.Helpers;
using LibraryManagement.Web.Interfaces;
using LibraryManagement.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Web.Controllers
{
    [Authorize(Roles = "Librarian")]
    public class LibrariansController : Controller
    {
        private readonly IRepository<Librarian> _librarianRepo;
        private readonly IRepository<Login> _loginRepo;

        public LibrariansController(IRepository<Librarian> librarianRepo, IRepository<Login> loginRepo)
        {
            _librarianRepo = librarianRepo;
            _loginRepo = loginRepo;
        }

        // GET: Librarians
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["EmpSortParm"] = sortOrder == "Emp" ? "emp_desc" : "Emp";

            if (searchString != null) pageNumber = 1;
            else searchString = currentFilter;

            ViewData["CurrentFilter"] = searchString;

            // PERFORMANCE: AsNoTracking is automatically applied in IRepository
            var librarians = _librarianRepo.GetQueryable().AsNoTracking();

            // Search
            if (!String.IsNullOrEmpty(searchString))
            {
                librarians = librarians.Where(l => l.Name.Contains(searchString)
                                       || l.EmployeeId.Contains(searchString)
                                       || l.Email.Contains(searchString));
            }

            // Sorting
            librarians = sortOrder switch
            {
                "name_desc" => librarians.OrderByDescending(l => l.Name),
                "Emp" => librarians.OrderBy(l => l.EmployeeId),
                "emp_desc" => librarians.OrderByDescending(l => l.EmployeeId),
                _ => librarians.OrderBy(l => l.Name),
            };

            int pageSize = 5;
            return View(await PaginatedList<Librarian>.CreateAsync(librarians, pageNumber ?? 1, pageSize));
        }

        // GET: Librarians/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var librarian = await _librarianRepo.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (librarian == null) return NotFound();

            return View(librarian);
        }

        // GET: Librarians/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Librarians/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Librarian librarian)
        {
            if (ModelState.IsValid)
            {
                // Auto-generate Login Record
                var login = new Login
                {
                    Username = librarian.EmployeeId,
                    PasswordHash = "password123",
                    Role = "Librarian"
                };

                await _loginRepo.AddAsync(login);
                await _loginRepo.SaveChangesAsync(); // To get the Login ID generated

                librarian.LoginId = login.Id;
                await _librarianRepo.AddAsync(librarian);
                await _librarianRepo.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Librarian profile and login created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(librarian);
        }

        // GET: Librarians/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var librarian = await _librarianRepo.GetByIdAsync(id.Value);
            if (librarian == null) return NotFound();
            
            return View(librarian);
        }

        // POST: Librarians/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Librarian librarian)
        {
            if (id != librarian.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingLibrarian = await _librarianRepo.GetQueryable().AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
                    if (existingLibrarian != null)
                    {
                        librarian.LoginId = existingLibrarian.LoginId;
                    }

                    _librarianRepo.Update(librarian);
                    await _librarianRepo.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Librarian details updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LibrarianExists(librarian.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(librarian);
        }

        // POST: Librarians/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var librarian = await _librarianRepo.GetByIdAsync(id);
            if (librarian != null)
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                if (userIdClaim != null && int.Parse(userIdClaim) == librarian.LoginId)
                {
                    TempData["ErrorMessage"] = "You cannot delete your own account.";
                    return RedirectToAction(nameof(Index));
                }

                var login = await _loginRepo.GetByIdAsync(librarian.LoginId);
                
                _librarianRepo.Remove(librarian);
                if (login != null) _loginRepo.Remove(login);
                
                await _librarianRepo.SaveChangesAsync();
                TempData["SuccessMessage"] = "Librarian successfully deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool LibrarianExists(int id)
        {
            return _librarianRepo.GetQueryable().Any(e => e.Id == id);
        }
    }
}
