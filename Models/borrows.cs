namespace INF272_HW_Assignment_2.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class borrows
    {
        public int borrowId { get; set; }
        public Nullable<int> studentId { get; set; }
        public Nullable<int> bookId { get; set; }
        public Nullable<System.DateTime> takenDate { get; set; }
        public Nullable<System.DateTime> broughtDate { get; set; }
    
        public virtual books books { get; set; }
        public virtual students students { get; set; }
    }
}
