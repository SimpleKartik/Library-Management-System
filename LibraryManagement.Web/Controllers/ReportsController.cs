using ClosedXML.Excel;
using LibraryManagement.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Web.Controllers
{
    [Authorize(Roles = "Librarian")]
    public class ReportsController : Controller
    {
        private readonly LibraryDbContext _context;

        public ReportsController(LibraryDbContext context)
        {
            _context = context;
        }

        // Export Borrow History to Excel
        [HttpGet]
        public async Task<IActionResult> ExportBorrowHistoryExcel()
        {
            var records = await _context.BorrowRecords
                .Include(r => r.Book)
                .Include(r => r.Student)
                .OrderByDescending(r => r.BorrowDate)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Borrow History");
                var currentRow = 1;

                // Headers
                worksheet.Cell(currentRow, 1).Value = "ID";
                worksheet.Cell(currentRow, 2).Value = "Student Name";
                worksheet.Cell(currentRow, 3).Value = "Book Title";
                worksheet.Cell(currentRow, 4).Value = "Borrow Date";
                worksheet.Cell(currentRow, 5).Value = "Due Date";
                worksheet.Cell(currentRow, 6).Value = "Return Date";
                worksheet.Cell(currentRow, 7).Value = "Status";

                var headerRange = worksheet.Range(1, 1, 1, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Data
                foreach (var record in records)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = record.Id;
                    worksheet.Cell(currentRow, 2).Value = record.Student?.Name ?? "N/A";
                    worksheet.Cell(currentRow, 3).Value = record.Book?.Title ?? "N/A";
                    worksheet.Cell(currentRow, 4).Value = record.BorrowDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(currentRow, 5).Value = record.DueDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(currentRow, 6).Value = record.ReturnDate.HasValue ? record.ReturnDate.Value.ToString("yyyy-MM-dd") : "";
                    worksheet.Cell(currentRow, 7).Value = record.Status;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BorrowHistory_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }
    }
}
