using INF272_HW_Assignment_2.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;


namespace INF272_HW_Assignment_2.Controllers
{
    public class HomeController : Controller
    {

        private LibraryEntities db = new LibraryEntities();
        public async Task<ActionResult> Index(int studentPageNumber = 1, int bookPageNumber = 1, int pageSize = 10)
        {
            // Fetch students with pagination
            var studentsQuery = db.students.OrderBy(s => s.name);
            var totalStudents = await studentsQuery.CountAsync();
            var students = await studentsQuery
                .Skip((studentPageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Fetch books with authors using eager loading
            var booksQuery = db.books.Include(b => b.authors).OrderBy(b => b.name);
            var totalBooks = await booksQuery.CountAsync();
            var books = await booksQuery
                .Skip((bookPageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Create model after fetching data
            var model = new LibraryViewModel
            {
                students = students,
                books = books,
                TotalStudents = totalStudents,
                TotalBooks = totalBooks,
                TotalStudentsPages = (int)Math.Ceiling((double)totalStudents / pageSize),
                TotalBooksPages = (int)Math.Ceiling((double)totalBooks / pageSize),
                CurrentStudentPage = studentPageNumber,
                CurrentBookPage = bookPageNumber,
                StartPage = (studentPageNumber - 1) / 10 * 10 + 1,
                EndPage = Math.Min((studentPageNumber - 1) / 10 * 10 + 10, (int)Math.Ceiling((double)totalStudents / pageSize)),
                BKStartPage = (bookPageNumber - 1) / 10 * 10 + 1,
                BKEndPage = Math.Min((bookPageNumber - 1) / 10 * 10 + 10, (int)Math.Ceiling((double)totalBooks / pageSize)),
            };

            return View(model);
        }

        public async Task<ActionResult> Maintain(int authorPageNumber = 1, int typesPageNumber = 1, int borrowsPageNumber = 1, int pageSize = 10)
        {
            // Fetch students with pagination
            var authorsQuery = db.authors.OrderBy(s => s.name);
            var totalAuthors = await authorsQuery.CountAsync();
            var authors = await authorsQuery
                .Skip((authorPageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Fetch books with authors using eager loading
            var typesQuery = db.types.OrderBy(b => b.name);
            var totalTypes = await typesQuery.CountAsync();
            var types = await typesQuery
                .Skip((typesPageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var borrowsQuery = db.borrows.OrderBy(b => b.borrowId);
            var totalBorrows = await borrowsQuery.CountAsync();
            var borrows = await borrowsQuery
                .Skip((borrowsPageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Create model after fetching data
            var model = new LibraryViewModel
            {
                authors = authors,
                types = types,
                borrows = borrows,
                TotalAuthors = totalAuthors,
                TotalBorrows = totalBorrows,
                TotalTypes = totalTypes,
                TotalAuthorsPages = (int)Math.Ceiling((double)totalAuthors / pageSize),
                TotalBorrowsPages = (int)Math.Ceiling((double)totalBorrows / pageSize),
                TotalTypesPages = (int)Math.Ceiling((double)totalTypes / pageSize),
                CurrentAuthorPage = authorPageNumber,
                CurrentBorrowPage = borrowsPageNumber,
                CurrentTypePage = typesPageNumber,
                StartPage = (authorPageNumber - 1) / 10 * 10 + 1,
                EndPage = Math.Min((authorPageNumber - 1) / 10 * 10 + 10, (int)Math.Ceiling((double)totalAuthors / pageSize)),
                BStartPage = (borrowsPageNumber - 1) / 10 * 10 + 1,
                BEndPage = Math.Min((borrowsPageNumber - 1) / 10 * 10 + 10, (int)Math.Ceiling((double)totalBorrows / pageSize)),
            };

            return View(model);
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult Reports()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GenerateReport(DateTime startDate, DateTime endDate, string reportType, string fileName, string fileType)
        {
            List<object> reportdata;

            // Fetch data based on report type
            if (reportType == "PopularBooks")
            {
                reportdata = db.borrows
                    .Where(b => b.takenDate >= startDate && b.takenDate <= endDate)
                    .GroupBy(b => b.bookId)
                    .Select(g => new
                    {
                        BookName = db.books
                            .Where(book => book.bookId == g.Key)
                            .Select(book => book.name)
                            .FirstOrDefault() ?? "Unknown Book",
                        BorrowCount = g.Count()
                    })
                    .ToList<object>();
            }
            else if (reportType == "BorrowingHistory")
            {
                reportdata = db.borrows
                    .Where(b => b.takenDate >= startDate && b.takenDate <= endDate)
                    .GroupBy(b => b.studentId)
                    .Select(g => new
                    {
                        StudentName = db.students
                            .Where(student => student.studentId == g.Key)
                            .Select(student => student.name)
                            .FirstOrDefault() ?? "Unknown Student",
                        BorrowedBooks = g.Select(b => new
                        {
                            BookName = db.books
                                .Where(book => book.bookId == b.bookId)
                                .Select(book => book.name)
                                .FirstOrDefault() ?? "Unknown Book",
                            TakenDate = b.takenDate,
                            BroughtDate = b.broughtDate
                        }).ToList()
                    })
                    .ToList<object>();
            }

            else
            {
                return new HttpStatusCodeResult(400, "Invalid report type.");
            }

            // File generation Type
            if (fileType == "Pdf")
            {
                byte[] pdfFile = GeneratePdfReport(reportdata, reportType);
                string base64Pdf = Convert.ToBase64String(pdfFile); // Convert to base64
                return Json(new { FileData = base64Pdf, FileName = fileName + ".pdf", FileType = "application/pdf" });
            }
            else if (fileType == "Excel")
            {
                byte[] excelFile = GenerateExcelReport(reportdata, reportType);
                string base64Excel = Convert.ToBase64String(excelFile); // Convert to base64
                return Json(new { FileData = base64Excel, FileName = fileName + ".xlsx", FileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
            }

            return new HttpStatusCodeResult(400, "Invalid file type.");
        }

        // Method to generate the PDF report using iTextSharp
        private byte[] GeneratePdfReport(List<object> reportData, string reportType)
        {
            using (var memoryStream = new MemoryStream())
            {
                Document document = new Document();
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Add report title and headers
                document.Add(new Paragraph($"{reportType} Report"));
                document.Add(new Paragraph("Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

                PdfPTable table = new PdfPTable(1) ;

                if (reportType == "PopularBooks")
                {
                    table = new PdfPTable(2);
                    table.AddCell("Book Name");
                    table.AddCell("Borrow Count");

                    foreach (var item in reportData)
                    {
                        table.AddCell(((dynamic)item).BookName);
                        table.AddCell(((dynamic)item).BorrowCount.ToString());
                    }
                }
                else if (reportType == "BorrowingHistory")
                {
                    table = new PdfPTable(4);
                    table.AddCell("Student Name");
                    table.AddCell("Book Name");
                    table.AddCell("Taken Date");
                    table.AddCell("Brought Date");

                    foreach (var student in reportData)
                    {
                        foreach (var book in ((dynamic)student).BorrowedBooks)
                        {
                            table.AddCell(((dynamic)student).StudentName);
                            table.AddCell(((dynamic)book).BookName);
                            table.AddCell(((dynamic)book).TakenDate.ToString("yyyy-MM-dd"));
                            table.AddCell(((dynamic)book).BroughtDate?.ToString("yyyy-MM-dd") ?? "Not Returned");
                        }
                    }
                }

                document.Add(table);

                document.Close();

                return memoryStream.ToArray();
            }
        } 

            private byte[] GenerateExcelReport(List<object> reportData, string reportType)
        {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(reportType + " Report");

                if (reportType == "PopularBooks")
                {
                    worksheet.Cells[1, 1].Value = "Book Name";
                    worksheet.Cells[1, 2].Value = "Borrow Count";

                    int row = 2;
                    foreach (var item in reportData)
                    {
                        worksheet.Cells[row, 1].Value = ((dynamic)item).BookName;
                        worksheet.Cells[row, 2].Value = ((dynamic)item).BorrowCount;
                        row++;
                    }
                }
                else if (reportType == "BorrowingHistory")
                {
                    worksheet.Cells[1, 1].Value = "Student Name";
                    worksheet.Cells[1, 2].Value = "Book Name";
                    worksheet.Cells[1, 3].Value = "Taken Date";
                    worksheet.Cells[1, 4].Value = "Brought Date";

                    int row = 2;
                    foreach (var student in reportData)
                    {
                        foreach (var book in ((dynamic)student).BorrowedBooks)
                        {
                            worksheet.Cells[row, 1].Value = ((dynamic)student).StudentName;
                            worksheet.Cells[row, 2].Value = ((dynamic)book).BookName;
                            worksheet.Cells[row, 3].Value = ((dynamic)book).TakenDate?.ToString("yyyy-MM-dd");
                            worksheet.Cells[row, 4].Value = ((dynamic)book).BroughtDate?.ToString("yyyy-MM-dd") ?? "Not Returned";
                            row++;
                        }
                    }
                }

                return package.GetAsByteArray();
            }
        }


    }
}


