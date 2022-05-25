using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using WebApplication1.Models;


namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private string _connection;
        public HomeController()
        {
            _connection = ConfigurationManager.ConnectionStrings["DBConnection"].ToString();
        }

        [HttpGet]
        public ActionResult Search(string titleFilter, int page)
        {
            {
                List<Books> books = new List<Books>();
                List<booksdto> bdt = new List<booksdto>();
                SearchBooks srcbooks = new SearchBooks();
                Books book = new Models.Books();


                string query = $"select b.id, b.title , b.description, publisher_id , authors_id, p.publisher_name, a.authors_name from \"Books\" b join \"Publisher\" p on b.publisher_id = p.id  join \"Authors\" a on b.authors_id = a.id Order By b.id limit 10 offset ({page}-1)*10";
                
                if (!String.IsNullOrWhiteSpace(titleFilter))
                    query = $"select b.id, b.title , b.description, publisher_id , authors_id, p.publisher_name, a.authors_name from \"Books\" b join \"Publisher\" p on b.publisher_id = p.id  join \"Authors\" a on b.authors_id = a.id  where b.title like '{titleFilter}%' Order By b.id limit 10 offset ({page}-1)*10";

                NpgsqlConnection conn = new NpgsqlConnection(_connection);
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {   
                    srcbooks.books.Add(new booksdto()
                    {
                        id = Convert.ToInt32(reader["id"]),
                        title = reader["title"].ToString(),
                        description = reader["description"].ToString(),
                        publisher_id = Convert.ToInt32(reader["publisher_id"]),
                        authors_id = Convert.ToInt32(reader["authors_id"]),
                        publisher_name = reader["publisher_name"].ToString(),
                        authors_name = reader["authors_name"].ToString(),
                        
                    });
                    
                }
                srcbooks.bookscount = getPagination(titleFilter);
                return Json(srcbooks, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            EditBooksViewModel vm = new EditBooksViewModel();
            Books book = new Books();
            
            string query = $"select b.id, b.title, b.publisher_id, b.authors_id, b.description from \"Books\" b where b.id = {id}";
            NpgsqlConnection conn = new NpgsqlConnection(_connection);
            conn.Open();
            NpgsqlCommand command = new NpgsqlCommand(query, conn);
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                vm.book.id = Convert.ToInt32(reader["id"]);
                vm.book.title = reader["title"].ToString();
                vm.book.description = reader["description"].ToString();
                vm.book.publisher_id = Convert.ToInt32(reader["publisher_id"]);
                vm.book.authors_id = Convert.ToInt32(reader["authors_id"]);
            }
            vm.publishers = getPublisher();
            vm.authors = getAuthors();
            
            vm.categories = getCategory();
            vm.book.categoriesIds = getBookCategoryIds(vm.book.id);
            
          
            return View(vm);    
        }

        public List<int> getBookCategoryIds(int id)
        {
            EditBooksViewModel vm = new EditBooksViewModel();
            
            List<int> categoriesIds = new List<int>();

            string query = $"select bc.category_id from \"BooksCategory\" bc where books_id = {id};";

            NpgsqlConnection conn = new NpgsqlConnection(_connection);
            conn.Open();
            NpgsqlCommand command = new NpgsqlCommand(query, conn);
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                categoriesIds.Add(Convert.ToInt32(reader["category_id"]));
                   
            };
            return (categoriesIds);
        }

        public List<Authors> getAuthors()
        {
            List<Authors> authors = new List<Authors>();

            string query = $"select * from \"Authors\" b";
            NpgsqlConnection conn = new NpgsqlConnection(_connection);
            conn.Open();
            NpgsqlCommand command = new NpgsqlCommand(query, conn);
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                authors.Add(new Authors
                {
                    id = Convert.ToInt32(reader["id"]),
                    authors_name = reader["authors_name"].ToString()

                });
            }  
            return authors;
        }

        public List<Category> getCategory()
        {
            EditBooksViewModel vm = new EditBooksViewModel();
            List<Category> categories = new List<Category>();

            string query = $"select  c.id, c.\"name\" category_name from \"Category\" c";
            NpgsqlConnection conn = new NpgsqlConnection(_connection);
            conn.Open();
            NpgsqlCommand command = new NpgsqlCommand(query, conn);
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
               categories.Add(new Category
                {
                    id = Convert.ToInt32(reader["id"]),
                    category_name = reader["category_name"].ToString()
                });
            }
            return categories;
        }

        public List<Models.Publisher> getPublisher()
        {
            List<Models.Publisher> publishers = new List<Models.Publisher>();

            string query = $"select * from \"Publisher\" b";
            NpgsqlConnection conn = new NpgsqlConnection(_connection);
            conn.Open();
            NpgsqlCommand command = new NpgsqlCommand(query, conn);
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                publishers.Add(new Models.Publisher
                {
                    id = Convert.ToInt32(reader["id"]),
                    publisher_name = reader["publisher_name"].ToString()
                });
            }
            return publishers;
        }

        [HttpPost]
        public ActionResult EditSubmit(booksdto book)
        {
            EditBooksViewModel vm = new EditBooksViewModel();

            string query = "";
            
            if (book.id > 0)
            {
                query = $"update \"Books\" set title = '{book.title}', description = '{book.description}', authors_id = {book.authors_id}, publisher_id = {book.publisher_id}, last_date = now() where id = {book.id} returning *";    
            }
            else
            {
                query = $"insert into \"Books\" (title, description, create_date, last_Date, authors_id, publisher_id) values ('{book.title}', '{book.description}',now(), now(), {book.authors_id}, {book.publisher_id}) returning *";
            }

            NpgsqlConnection conn = new NpgsqlConnection(_connection);
            conn.Open();
            NpgsqlCommand command = new NpgsqlCommand(query, conn);
            NpgsqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                vm.book.id = Convert.ToInt32(reader["id"]);
                vm.book.title = reader["title"].ToString();
                vm.book.description = reader["description"].ToString();
                vm.book.publisher_id = Convert.ToInt32(reader["publisher_id"]);
                vm.book.authors_id = Convert.ToInt32(reader["authors_id"]);
            }

            vm.authors = getAuthors();
            vm.publishers = getPublisher();
            vm.categories = getCategory();
            getDeleteCategories(vm.book.id);
            getInsertCategories(vm.book.id, book.categoriesIds);

            return View("Edit", vm);
        }
      
        public ActionResult getDeleteCategories(int id)
        {
                string query = $"delete from \"BooksCategory\" where books_id ={id} returning id";
                NpgsqlConnection conn = new NpgsqlConnection(_connection);
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                NpgsqlDataReader reader = command.ExecuteReader();

                return this.Json(true);
                
        }

        public ActionResult getInsertCategories(int bookId, List<int> categoryIds)
        {
            foreach (int categoryId in categoryIds)
            {
                string query = $"insert into \"BooksCategory\" (books_id, category_id) values ({bookId}, {categoryId}) returning *";
                NpgsqlConnection conn = new NpgsqlConnection(_connection);
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                NpgsqlDataReader reader = command.ExecuteReader();
            }
            return this.Json(true);
            
        }

        //[HttpDelete]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                string query = $"delete from \"Books\" where id = {id}";
                NpgsqlConnection conn = new NpgsqlConnection(_connection);
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                NpgsqlDataReader reader = command.ExecuteReader();

                return this.Json(true);
            }
            catch
            {
                return this.Json(false);
            }
        }

        public int getPagination(string titleFilter)
        {
            int booksCount = 0;
            string query = $"select count(b.id) bookscount from \"Books\" b ";
            if (!String.IsNullOrWhiteSpace(titleFilter))
                query = $"select count(b.id) bookscount from \"Books\" b  where b.title like '{titleFilter}%' ";

            NpgsqlConnection conn = new NpgsqlConnection(_connection);
            conn.Open();
            NpgsqlCommand command = new NpgsqlCommand(query, conn);
            NpgsqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                booksCount = Convert.ToInt32(reader["bookscount"]);
            };
 
                return booksCount;
            }
        

        public ActionResult Index()
        {

            return View();
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
    }
}
    
