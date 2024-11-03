using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INF272_HW_Assignment_2.Models
{
    public class ReportViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportType { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }

        public List<ReportViewModel> ReportData { get; set; }
    }
}