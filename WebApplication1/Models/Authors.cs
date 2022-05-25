using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Authors
    {
        public int id { get; set; }
        public string authors_name { get; set; }
        public DateTime create_date { get; set; }
        public DateTime last_date { get; set; }
    }
}