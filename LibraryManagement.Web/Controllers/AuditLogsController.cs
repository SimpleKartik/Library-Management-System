using LibraryManagement.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Web.Helpers;

namespace LibraryManagement.Web.Controllers
{
    [Authorize(Roles = "Librarian")]
    public class AuditLogsController : Controller
    {
        private readonly LibraryDbContext _context;

        public AuditLogsController(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? pageNumber)
        {
            var logs = _context.ActivityLogs.OrderByDescending(l => l.Timestamp).AsQueryable();
            int pageSize = 15;
            return View(await PaginatedList<LibraryManagement.Web.Models.ActivityLog>.CreateAsync(logs.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
    }
}
