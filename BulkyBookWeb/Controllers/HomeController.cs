using BulkyBookWeb.Data;
using BulkyBookWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BulkyBookWeb.Controllers
{
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext _context;

		public HomeController(ApplicationDbContext context)
		{
			_context = context;
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

		public async Task<IActionResult> Index()
		{
			List<Library> libraryList = await _context.Libraries.AsNoTracking().ToListAsync();
			return View(libraryList);
		}
		[HttpPost]
		public async Task<IActionResult> Index(IFormFile file)
		{
			var fileName = file.FileName.ToString().Split(".")[0];
			var fileExt = file.FileName.ToString().Split(".")[1];
			var allowedExt = new string[] { "xlsx", "xls", "csv" };
			if (!allowedExt.Contains(fileExt))
			{
				TempData["InvalidFile"] = "Invalid File Extension";
				RedirectToAction("Index");
			}
			var filepath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files");
			if (!Directory.Exists(filepath))
			{
				Directory.CreateDirectory(filepath);
			}
			string excelPath = $"{fileName}{Guid.NewGuid().ToString().ToLower().Replace("-", "")}.{fileExt}";
			if (file.Length > 0)
			{
				using (FileStream stream = new(excelPath, FileMode.CreateNew))
				{
					await file.CopyToAsync(stream);
				}
				//Console.WriteLine(stream);
			}

			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}