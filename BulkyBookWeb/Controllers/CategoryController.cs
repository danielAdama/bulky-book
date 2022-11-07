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
        // Create
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
                TempData["Success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View(book);
        }
        // Edit
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = _dbContext.Categories.Find(id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        // Edit to Update
        [HttpPost]
        public IActionResult Edit(Category book)
        {
            if (book.Name == book.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name.");
            }
            if (ModelState.IsValid)
            {
                _dbContext.Categories.Update(book);
                _dbContext.SaveChanges();
                TempData["Success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View(book);
        }
        // Detlete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = _dbContext.Categories.Find(id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost]
        public IActionResult DeletePOST(int? id)
        {
            var book = _dbContext.Categories.Find(id);
            if (book == null)
            {
                return NotFound();
            }
            _dbContext.Categories.Remove(book);
            _dbContext.SaveChanges();
            TempData["Success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
