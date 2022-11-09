using BulkyBookWeb.Data;
using BulkyBookWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

		public async Task<IActionResult> Index(CancellationToken cancellationToken)
		{
			//var objCategoryList = _dbContext.Categories.ToList();
			// AsNoTracking() is only used when performing Read operation, but is not used when we are doing
			// CUD-(Create, Update and Delete)
			List<Category> objCategoryList = await _context.Categories.AsNoTracking().ToListAsync(cancellationToken);
			return View(objCategoryList);
		}
		public ViewResult Create()
		{
			return View();
		}
		// Create
		[HttpPost]
		public async Task<IActionResult> Create(Category book, CancellationToken cancellationToken=default)
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
				await _context.Categories.AddAsync(book, cancellationToken);
				await _context.SaveChangesAsync(cancellationToken);
				TempData["Success"] = "Category created successfully";
				return RedirectToAction("Index");
			}
			return View(book);
		}
		// Edit
		public async Task <IActionResult> Edit(long? id)
		{
			if (id == null || id == 0)
			{
				return NotFound();
			}
			var categoryFromDb = await _context.Categories.FindAsync(id);

			if (categoryFromDb == null)
			{
				return NotFound();
			}
			return View(categoryFromDb);
		}
		// Edit to Update
		[HttpPost]
		public async Task<IActionResult> Edit(Category book, CancellationToken cancellationToken=default)
		{
			if (book.Name == book.DisplayOrder.ToString())
			{
				ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name.");
			}
			if (ModelState.IsValid)
			{
				_context.Categories.Update(book);
				await _context.SaveChangesAsync(cancellationToken);
				TempData["Success"] = "Category updated successfully";
				return RedirectToAction("Index");
			}
			return View(book);
		}
		// Detlete
		public async Task<IActionResult> Delete(long? id)
		{
			if (id == null || id == 0)
			{
				return NotFound();
			}
			var categoryFromDb = await _context.Categories.FindAsync(id);

			if (categoryFromDb == null)
			{
				return NotFound();
			}
			return View(categoryFromDb);
		}
		[HttpPost]
		public async Task<IActionResult> DeletePOST(long? id, CancellationToken cancellationToken)
		{
			var book = await _context.Categories.FindAsync(id);
			if (book == null)
			{
				return NotFound();
			}
			_context.Categories.Remove(book);
			await _context.SaveChangesAsync(cancellationToken);
			TempData["Success"] = "Category deleted successfully";
			return RedirectToAction("Index");
		}
	}
}