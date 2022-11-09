using BulkyBookWeb.Data;
using BulkyBookWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Controllers
{
	[AutoValidateAntiforgeryToken]
	public class CategoryController : Controller
	{
		private readonly ApplicationDbContext _context;
		public CategoryController(ApplicationDbContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			//var objCategoryList = _dbContext.Categories.ToList();
			List<Category> objCategoryList = _context.Categories.ToList();
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
				_context.Categories.Add(book);
				_context.SaveChanges();
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
			var categoryFromDb = _context.Categories.Find(id);

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
				_context.Categories.Update(book);
				_context.SaveChanges();
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
			var categoryFromDb = _context.Categories.Find(id);

			if (categoryFromDb == null)
			{
				return NotFound();
			}
			return View(categoryFromDb);
		}
		[HttpPost]
		public IActionResult DeletePOST(int? id)
		{
			var book = _context.Categories.Find(id);
			if (book == null)
			{
				return NotFound();
			}
			_context.Categories.Remove(book);
			_context.SaveChanges();
			TempData["Success"] = "Category deleted successfully";
			return RedirectToAction("Index");
		}
	}
}