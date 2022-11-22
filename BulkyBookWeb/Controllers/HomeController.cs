using BulkyBookWeb.Data;
using BulkyBookWeb.Models;
using BulkyBookWeb.ViewModels;
using ExcelDataReader;
using ExcelDataReader.Log;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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


					using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken); // Track all operations
					try
					{
						ConcurrentBag<SyncLog> syncLogBag = new ConcurrentBag<SyncLog>();
						foreach (DataRow row in data.Rows)
						{
							SyncLog syncLog = new SyncLog()
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
							//await _context.SyncLogs.AddAsync(libraryLog, cancellationToken);
							//await _context.SaveChangesAsync(cancellationToken);
							// Store syncLog as concurrentbag so we can easily compare it with the librarybag
							syncLogBag.Add(syncLog);
						}


						// Compare data btw Libraries(bag) and SyncLogs table (bag), if change is detected add it to the ChangesLog table
						List<Library> libraryList = await _context.Libraries.ToListAsync(cancellationToken);

						ConcurrentBag<Library> newLibrarybag = new ConcurrentBag<Library>();
						ConcurrentBag<Library> updatedLibrarybag = new ConcurrentBag<Library>();
						ConcurrentBag<ChangesLog> changesLibrarybag = new ConcurrentBag<ChangesLog>();

						foreach (var log in syncLogBag)
						{
							var userLibraryData = libraryList.FirstOrDefault(x => x.UserId?.ToLower().Equals(log.UserId?.ToLower()) == true);
							// If the userLibraryData is false means a new book is been stored otherwise check previous books
							// for changes, when change is detected the system should flag that row of those values that changed.
							if (null == userLibraryData) // false
							{
								newLibrarybag.Add(new Library
								{
									UserId = log.UserId,
									Name = log.Name,
									DisplayOrder = log.DisplayOrder,
									Genre = log.Genre,
									ISBN = log.ISBN,
									Author = log.Author,
									Publisher = log.Publisher,
									TimeCreated = DateTime.Now,
									TimeUpdated = DateTime.Now,
								});
							}
							else // true
							{
								var libraryChanges = GetLibraryChanges(userLibraryData, log, out int numChanges);

								ChangesLog libChanges = new ChangesLog
								{
									Changes = JsonConvert.SerializeObject(libraryChanges),
									refCode = syncRef,
									RowAffected = numChanges,
									SyncId = syncId,
									TimeCreated = DateTime.Now,
									TimeUpdated = DateTime.Now,
									UserId = log.UserId,
								};

								changesLibrarybag.Add(libChanges);
								updatedLibrarybag.Add(userLibraryData);

							}

						};
						await _context.SyncLogs.AddRangeAsync(syncLogBag, cancellationToken);
						await _context.ChangesLogs.AddRangeAsync(changesLibrarybag, cancellationToken);

						if (newLibrarybag.Any())
						{
							await _context.Libraries.AddRangeAsync(newLibrarybag, cancellationToken);
						}
						if (updatedLibrarybag.Any())
						{
							_context.Libraries.UpdateRange(updatedLibrarybag);
						}
						await _context.SaveChangesAsync();

						await transaction.CommitAsync(cancellationToken);
						return RedirectToAction("Index");
					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync(cancellationToken);
						TempData["errorMessage"] = ex.Message;
						return RedirectToAction("Index");
					}

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

		private static bool IsNotNull([NotNullWhen(true)] object? obj) => obj != null; // Method to ignore null values

		public static Dictionary<string, Dictionary<string, string>> GetLibraryChanges(Library userLibraryData, SyncLog log, out int numChanges)
		{
			var detectedChanges = new Dictionary<string, Dictionary<string, string>>();
			//int numChanges = 0;
			numChanges = 0;

			if (IsNotNull(userLibraryData.Author) && IsNotNull(log.Author))
			{
				bool IsAuthor = !userLibraryData.Author.Equals(log.Author);
				if (IsAuthor)
				{
					detectedChanges.Add("Author", new Dictionary<string, string>
								{
									{ "Old", userLibraryData.Author.ToString() },
									{ "New", log.Author.ToString() }
								});
					//userLibraryData.Author = log.Author;
					userLibraryData.TimeUpdated = DateTime.Now;
					numChanges += 1;
				}
			}
			//bool IsAuthor = !userLibraryData.Author.Equals(log.Author);
			//if (IsAuthor)
			//{
			//	detectedChanges.Add("Author", new Dictionary<string, string>
			//					{
			//						{ "Old", userLibraryData.Author.ToString() },
			//						{ "New", log.Author.ToString() }
			//					});
			//	//userLibraryData.Author = log.Author;
			//	userLibraryData.TimeUpdated = DateTime.Now;
			//	numChanges += 1;
			//}

			if (IsNotNull(userLibraryData.Name) && IsNotNull(log.Name))
			{
				bool IsName = !userLibraryData.Name.Equals(log.Name);
				if (IsName)
				{
					detectedChanges.Add("Name", new Dictionary<string, string>
								{
									{ "Old", userLibraryData.Name.ToString() },
									{ "New", log.Name.ToString() }
								});
					//userLibraryData.Name = log.Name;
					userLibraryData.TimeUpdated = DateTime.Now;
					numChanges += 1;
				}
			}

			//bool IsName = !userLibraryData.Name.Equals(log.Name);
			//if (IsName)
			//{
			//	detectedChanges.Add("Name", new Dictionary<string, string>
			//					{
			//						{ "Old", userLibraryData.Name.ToString() },
			//						{ "New", log.Name.ToString() }
			//					});
			//	//userLibraryData.Name = log.Name;
			//	userLibraryData.TimeUpdated = DateTime.Now;
			//	numChanges += 1;
			//}

			bool IsDisplayOrder = !userLibraryData.DisplayOrder.Equals(log.DisplayOrder);
			if (IsDisplayOrder)
			{
				detectedChanges.Add("DisplayOrder", new Dictionary<string, string>
								{
									{ "Old", userLibraryData.DisplayOrder.ToString() },
									{ "New", log.DisplayOrder.ToString() }
								});
				//userLibraryData.DisplayOrder = log.DisplayOrder;
				userLibraryData.TimeUpdated = DateTime.Now;
				numChanges += 1;
			}

			if (IsNotNull(userLibraryData.Genre) && IsNotNull(log.Genre))
			{
				bool IsGenre = !userLibraryData.Genre.Equals(log.Genre);
				if (IsGenre)
				{
					detectedChanges.Add("Genre", new Dictionary<string, string>
								{
									{ "Old", userLibraryData.Genre.ToString() },
									{ "New", log.Genre.ToString() }
								});
					//userLibraryData.Genre = log.Genre;
					userLibraryData.TimeUpdated = DateTime.Now;
					numChanges += 1;
				}
			}
			//bool IsGenre = !userLibraryData.Genre.Equals(log.Genre);
			//if (IsGenre)
			//{
			//	detectedChanges.Add("Genre", new Dictionary<string, string>
			//					{
			//						{ "Old", userLibraryData.Genre.ToString() },
			//						{ "New", log.Genre.ToString() }
			//					});
			//	//userLibraryData.Genre = log.Genre;
			//	userLibraryData.TimeUpdated = DateTime.Now;
			//	numChanges += 1;
			//}

			if (IsNotNull(userLibraryData.ISBN) && IsNotNull(log.ISBN))
			{
				bool IsISBN = !userLibraryData.ISBN.Equals(log.ISBN);
				if (IsISBN)
				{
					detectedChanges.Add("ISBN", new Dictionary<string, string>
								{
									{ "Old", userLibraryData.ISBN.ToString() },
									{ "New", log.ISBN.ToString() }
								});
					//userLibraryData.ISBN = log.ISBN;
					userLibraryData.TimeUpdated = DateTime.Now;
					numChanges += 1;
				}
			}

			if (IsNotNull(userLibraryData.Publisher) && IsNotNull(log.Publisher))
			{
				bool IsPublisher = !userLibraryData.Publisher.Equals(log.Publisher);
				if (IsPublisher)
				{
					detectedChanges.Add("Publisher", new Dictionary<string, string>
								{
									{ "Old", userLibraryData.Publisher.ToString() },
									{ "New", log.Publisher.ToString() }
								});
					//userLibraryData.Publisher = log.Publisher;
					userLibraryData.TimeUpdated = DateTime.Now;
					numChanges += 1;
				}
			}

			//bool IsPublisher = !userLibraryData.Publisher.Equals(log.Publisher);
			//if (IsPublisher)
			//{
			//	detectedChanges.Add("Publisher", new Dictionary<string, string>
			//					{
			//						{ "Old", userLibraryData.Publisher.ToString() },
			//						{ "New", log.Publisher.ToString() }
			//					});
			//	//userLibraryData.Publisher = log.Publisher;
			//	userLibraryData.TimeUpdated = DateTime.Now;
			//	numChanges += 1;
			//}
			return detectedChanges;
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