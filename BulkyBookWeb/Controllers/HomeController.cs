using BulkyBookWeb.Data;
using BulkyBookWeb.Models;
using BulkyBookWeb.ViewModels;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Transactions;
using static System.Net.WebRequestMethods;

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

		public async Task<IActionResult> Index(DisplayBooksAndUploadFileViewModel bookVM)
		{
			//List<Library> libraryList = await _context.Libraries.AsNoTracking().ToListAsync();
			//return View(libraryList);

			bookVM.GetLibraries = await _context.Libraries.AsNoTracking().ToListAsync();
			return View(bookVM);
		}
		[HttpPost]
		public async Task<IActionResult> Index(DisplayBooksAndUploadFileViewModel fileVM, CancellationToken cancellationToken = default)
		{
			var fileName = fileVM.File.FileName.ToString().Split(".")[0];
			var fileExt = fileVM.File.FileName.ToString().Split(".")[1];
			var allowedExt = new string[] { "xlsx", "xls"};
			if (!allowedExt.Contains(fileExt))
			{
				TempData["InvalidFile"] = "Invalid File Extension";
				RedirectToAction("Index");
			}
			else
			{
				TempData["ValidFile"] = "File Successfully uploaded";
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files");
				if (!Directory.Exists(filePath))
				{
					Directory.CreateDirectory(filePath);
				}
				string path = $"{filePath}\\{fileName}{Guid.NewGuid().ToString().ToLower().Replace("-", "")}.{fileExt}";

				if (fileVM.File.Length > 0)
				{
					string syncRef = $"LBY-{Guid.NewGuid().ToString().ToLower().Replace("-", "")}";
					long syncId = await CreateSyncId(syncRef, cancellationToken);
					DataTable data = await GetExcelTable(path, fileVM, cancellationToken);
					System.IO.File.Delete(path);

					foreach (DataRow row in data.Rows)
					{
						SyncLog log = new SyncLog()
						{
							LibSyncId = syncId,
							UserId = row["UserId"].ToString(),
							Name = row["Name"].ToString(),
							DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
							Genre = row["Genre"].ToString(),
							ISBN = row["ISBN"].ToString(),
							Author = row["Author"].ToString(),
							Publisher = row["Publisher"].ToString(),

						};
						await _context.SyncLogs.AddAsync(log, cancellationToken);
						await _context.SaveChangesAsync(cancellationToken);
					}

					// Compare data btw Libraries and SyncLogs table, if change is detected add it to the ChangesLog table
					


				}
			}
			fileVM.GetLibraries = await _context.Libraries.AsNoTracking().ToListAsync(cancellationToken);
			return View(fileVM);
		}

		public async Task<DataTable> GetExcelTable(string path, DisplayBooksAndUploadFileViewModel file, CancellationToken cancellationToken)
		{
			using (FileStream stream = new(path, FileMode.Create))
			{
				await file.File.CopyToAsync(stream, cancellationToken);
			}
			var fileStream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read);
			IExcelDataReader reader = ExcelReaderFactory.CreateReader(fileStream);

			using var data = reader.AsDataSet(new ExcelDataSetConfiguration()
			{
				UseColumnDataType = true,
				FilterSheet = (tableReader, sheetIndex) => true,
				ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration
				{
					UseHeaderRow = true,
					FilterRow = (rowReader) => true,
					FilterColumn = (rowReader, columnIndex) => true,
				}
			}).Tables[0] ?? new System.Data.DataTable(); // Return the Excel (Left side) if it is not null otherwise return the right side
			reader.Close();

			return data;
		}

		public async Task<long> CreateSyncId(string syncRef, CancellationToken cancellationToken = default)
		{
			Sync refId = new Sync
			{
				Reference = syncRef,
			};
			await _context.Syncs.AddAsync(refId, cancellationToken);
			await _context.SaveChangesAsync(cancellationToken);
			return refId.Id;
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