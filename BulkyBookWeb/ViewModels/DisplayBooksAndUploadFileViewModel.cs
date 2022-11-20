using BulkyBookWeb.ViewModels;
using BulkyBookWeb.Models;

namespace BulkyBookWeb.ViewModels
{
	public class DisplayBooksAndUploadFileViewModel : ResponseViewModel
	{
		// This property returns the list of books (GetLibraries) in the library and also
		// aids file uploads (File)
		public List<Library> GetLibraries { get; set; }
		public IFormFile File { get; set; }
	}
}
