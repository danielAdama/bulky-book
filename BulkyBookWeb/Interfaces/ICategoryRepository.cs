using BulkyBookWeb.Models;

namespace BulkyBookWeb.Interfaces
{
	public interface ICategoryRepository
	{
		Task<List<Category>> GetAllCategoriesAsync();
		Task<Category> GetIdAsync(long id);
		Task<int> GetCountAsync();
		bool Add(Category category);
		bool Update(Category category);
		bool Delete(Category category);
		bool Save();
	}
}
