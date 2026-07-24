using LibraryManagement.Web.Helpers;
using LibraryManagement.Web.Interfaces;
using LibraryManagement.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Web.Controllers
{
    [Authorize(Roles = "Librarian")]
    public class StudentsController : Controller
    {
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<Login> _loginRepo;

        public StudentsController(IRepository<Student> studentRepo, IRepository<Login> loginRepo)
        {
            _studentRepo = studentRepo;
            _loginRepo = loginRepo;
        }

        // GET: Students
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["RegSortParm"] = sortOrder == "Reg" ? "reg_desc" : "Reg";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var students = _studentRepo.GetQueryable().AsNoTracking();

            // Search
            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.Name.Contains(searchString)
                                       || s.RegistrationNumber.Contains(searchString)
                                       || s.Email.Contains(searchString));
            }

            // Sorting
            students = sortOrder switch
            {
                "name_desc" => students.OrderByDescending(s => s.Name),
                "Reg" => students.OrderBy(s => s.RegistrationNumber),
                "reg_desc" => students.OrderByDescending(s => s.RegistrationNumber),
                _ => students.OrderBy(s => s.Name),
            };

            int pageSize = 10;
            return View(await PaginatedList<Student>.CreateAsync(students, pageNumber ?? 1, pageSize));
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _studentRepo.GetQueryable()
                .Include(s => s.BorrowRecords!)
                    .ThenInclude(br => br.Book)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null) return NotFound();

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (ModelState.IsValid)
            {
                // Auto-generate Login Record
                var login = new Login
                {
                    Username = student.RegistrationNumber, // Default username
                    PasswordHash = "password123", // Default password
                    Role = "Student"
                };

                await _loginRepo.AddAsync(login);
                await _loginRepo.SaveChangesAsync();

                student.LoginId = login.Id;
                await _studentRepo.AddAsync(student);
                await _studentRepo.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Student profile created and login generated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _studentRepo.GetByIdAsync(id.Value);
            if (student == null) return NotFound();
            
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure we don't accidentally wipe out LoginId if it wasn't bound
                    var existingStudent = await _studentRepo.GetQueryable().AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
                    if (existingStudent != null)
                    {
                        student.LoginId = existingStudent.LoginId;
                    }

                    _studentRepo.Update(student);
                    await _studentRepo.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Student profile updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _studentRepo.GetByIdAsync(id);
            if (student != null)
            {
                // Also delete the login record
                var login = await _loginRepo.GetByIdAsync(student.LoginId);
                
                _studentRepo.Remove(student);
                if (login != null) _loginRepo.Remove(login);
                
                await _studentRepo.SaveChangesAsync();
                TempData["SuccessMessage"] = "Student successfully deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _studentRepo.GetQueryable().Any(e => e.Id == id);
        }
    }
}
