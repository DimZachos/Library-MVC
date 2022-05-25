using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class BooksCategory
    {
        public int id { get; set; }
        public int books_id { get; set; }
        public int category_id { get; set; }
        public DateTime create_date { get; set; }
        public DateTime last_date { get; set; }

    }
}