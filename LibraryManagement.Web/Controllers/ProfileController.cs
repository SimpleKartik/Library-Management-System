using LibraryManagement.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly LibraryDbContext _context;

        public ProfileController(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (userIdClaim == null) return Unauthorized();
            
            int loginId = int.Parse(userIdClaim);

            if (User.IsInRole("Librarian"))
            {
                var librarian = await _context.Librarians.FirstOrDefaultAsync(l => l.LoginId == loginId);
                ViewBag.Role = "Librarian";
                return View("LibrarianProfile", librarian);
            }
            else
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.LoginId == loginId);
                ViewBag.Role = "Student";
                return View("StudentProfile", student);
            }
        }
    }
}
