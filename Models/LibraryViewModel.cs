using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INF272_HW_Assignment_2.Models
{
    public class LibraryViewModel
    {
        public IEnumerable<students> students { get; set; }
        public IEnumerable<books> books { get; set; }

        public IEnumerable<authors> authors { get; set; }
        public IEnumerable<types> types { get; set; }
        public IEnumerable<borrows> borrows { get; set; }


        // Pagination properties for Students
        public int TotalStudents { get; set; }
        public int TotalStudentsPages { get; set; }
        public int CurrentStudentPage { get; set; }

        // Pagination properties for Books
        public int TotalBooks { get; set; }
        public int TotalBooksPages { get; set; }
        public int CurrentBookPage { get; set; }

        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public int BKStartPage { get; set; }
        public int BKEndPage { get; set; }
        public int BStartPage { get; set; }
        public int BEndPage { get; set; }

        public int CurrentTypePage { get; set; }
        public int TotalTypes { get; set; }
        public int TotalTypesPages { get; set; }

        public int CurrentAuthorPage { get; set; }
        public int TotalAuthors { get; set; }
        public int TotalAuthorsPages { get; set; }

        public int CurrentBorrowPage { get; set; }
        public int TotalBorrows { get; set; }
        public int TotalBorrowsPages { get; set; }
    }
}