using BulkyBookWeb.Data;
using BulkyBookWeb.Interfaces;
using BulkyBookWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace BulkyBookWeb.Repository
{
	public class CategoryRepository : ICategoryRepository
	{
		private readonly ApplicationDbContext _context;

		public CategoryRepository(ApplicationDbContext context)
		{
			_context = context;
		}
		public bool Add(Category category)
		{
			_context.Add(category);
			return Save();
		}

		public bool Delete(Category category)
		{
			_context.Remove(category);
			return Save();
		}

		public async Task<List<Category>> GetAllCategoriesAsync()
		{
			return await _context.Categories.AsNoTracking().ToListAsync();
		}

		public async Task<Category> GetIdAsync(long id)
		{
			return await _context.Categories.FindAsync(id);
		}

		public bool Save()
		{
			var saved = _context.SaveChanges();
			return saved > 0;
		}

		public bool Update(Category category)
		{
			_context.Update(category);
			return Save();
		}
	}
}
