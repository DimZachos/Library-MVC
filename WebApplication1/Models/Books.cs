using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Books
    {
        [Key]
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public DateTime create_date { get; set; }
        public DateTime last_date { get; set; }
        public int publisher_id { get; set; }
        public int authors_id { get; set; }
    }

    public class EditBooksViewModel  
    {
        public booksdto book = new booksdto();
        public List<Authors> authors = new List<Authors>();
        public List<Publisher> publishers = new List<Publisher>();
        public List<Category> categories = new List<Category>();

    }

    public class booksdto : Books
    {
        public string publisher_name {get; set;}
        public string authors_name {get; set;}
        public List<int> categoriesIds { get; set; }
        
    }
    public class SearchBooks 
    {
        public List<booksdto> books = new List<booksdto>(); 
        public int bookscount { get; set; }
    }
}