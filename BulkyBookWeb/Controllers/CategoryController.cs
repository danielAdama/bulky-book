using BulkyBookWeb.Data;
using BulkyBookWeb.Interfaces;
using BulkyBookWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace BulkyBookWeb.Controllers
{
	[AutoValidateAntiforgeryToken]
	public class CategoryController : Controller
	{
		private readonly ICategoryRepository _categoryRepository;

		public CategoryController(ICategoryRepository categoryRepository)
		{
			_categoryRepository = categoryRepository;
		}

		public async Task<IActionResult> Index()
		{
			//var objCategoryList = _dbContext.Categories.ToList();
			// AsNoTracking() is only used when performing Read operation, but is not used when we are doing
			// CUD-(Create, Update and Delete)
			List<Category> objCategoryList = await _categoryRepository.GetAllCategoriesAsync();
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
				_categoryRepository.Add(book);
				TempData["Success"] = "Category created successfully";
				return RedirectToAction("Index");
			}
			return View(book);
		}
		// Edit
		public async Task<IActionResult> Edit(long id)
		{
			Category categoryFromDb = await _categoryRepository.GetIdAsync(id);

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
				_categoryRepository.Update(book);
				TempData["Success"] = "Category updated successfully";
				return RedirectToAction("Index");
			}
			return View(book);
		}
		// Detlete
		public async Task<IActionResult> Delete(long id)
		{
			Category categoryFromDb = await _categoryRepository.GetIdAsync(id);

			if (categoryFromDb == null)
			{
				return NotFound();
			}
			return View(categoryFromDb);
		}
		[HttpPost]
		public async Task<IActionResult> DeletePOST(long id)
		{
			Category book = await _categoryRepository.GetIdAsync(id);
			if (book == null)
			{
				return NotFound();
			}
			_categoryRepository.Delete(book);
			TempData["Success"] = "Category deleted successfully";
			return RedirectToAction("Index");
		}
	}
}