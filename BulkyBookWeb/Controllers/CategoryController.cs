using BulkyBookWeb.Data;
using BulkyBookWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        public CategoryController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            //var objCategoryList = _dbContext.Categories.ToList();
            IEnumerable<Category> objCategoryList = _dbContext.Categories;
            return View(objCategoryList);
        }
        public ViewResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category book)
        {
            if (book.Name == book.DisplayOrder.ToString()) 
            {
                ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name.");
                //ModelState.AddModelError("CustomError", "The DisplayOrder cannot exactly match the Name.");
            }
            // Validate the Category model for the name and display order, both has to be valid before
            // data is stored in the database, as name is a required field.
            if (ModelState.IsValid) 
            {
                _dbContext.Categories.Add(book);
                _dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(book);
        }
    }
}
