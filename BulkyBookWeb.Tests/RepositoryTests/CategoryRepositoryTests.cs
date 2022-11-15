using BulkyBookWeb.Data;
using BulkyBookWeb.Interfaces;
using BulkyBookWeb.Models;
using BulkyBookWeb.Repository;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBookWeb.Tests.RepositoryTests
{
	public class CategoryRepositoryTests
	{
		Category dummyCategory = new Category()
		{
			UserId = "5",
			Name = "Book for test",
			DisplayOrder = 8,
			Genre = "Action",
			ISBN = "433-546-232-765",
			Author = "Badman123",
			Publisher = "Joseph Republic",
			DateTimeNow = DateTime.Now,
			TimeCreated = DateTime.Now,
			TimeUpdated = DateTime.Now,
		};

		private async Task<ApplicationDbContext> GetDbContext()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			var databaseContext = new ApplicationDbContext(options);
			databaseContext.Database.EnsureCreated();
			if (await databaseContext.Categories.CountAsync() < 0)
			{
				for (int i = 0; i < 10; i++)
				{
					databaseContext.Categories.Add(dummyCategory);
					await databaseContext.SaveChangesAsync();
				}
			}
			return databaseContext;
		}

		[Fact]
		public async void Test_CategoryRepository_Add_Returns_True_And_Count1()
		{
			// Arrange
			Category fakeCategoryToCheck = new Category()
			{
				UserId = "5",
				Name = "Book for test",
				DisplayOrder = 8,
				Genre = "Action",
				ISBN = "433-546-232-765",
				Author = "Badman123",
				Publisher = "Joseph Republic",
				DateTimeNow = DateTime.Now,
				TimeCreated = DateTime.Now,
				TimeUpdated = DateTime.Now,
			};
			var dbContext = await GetDbContext();
			var categoryRepository = new CategoryRepository(dbContext);
			
			// Act
			var result = categoryRepository.Add(fakeCategoryToCheck);
			var counts = categoryRepository.GetCountAsync();

			// Assert
			Assert.True(result);
			Assert.Equal(1, counts.Result);
		}

		[Fact]
		public async void Test_CategoryRepository_Delete_Returns_True_And_Count0()
		{
			// Arrange
			Category fakeCategoryToCheck = new Category()
			{
				UserId = "5",
				Name = "Book for test",
				DisplayOrder = 8,
				Genre = "Action",
				ISBN = "433-546-232-765",
				Author = "Badman123",
				Publisher = "Joseph Republic",
				DateTimeNow = DateTime.Now,
				TimeCreated = DateTime.Now,
				TimeUpdated = DateTime.Now,
			};
			var dbContext = await GetDbContext();
			var categoryRepository = new CategoryRepository(dbContext);

			// Act
			categoryRepository.Add(fakeCategoryToCheck);
			var result = categoryRepository.Delete(fakeCategoryToCheck);
			var counts = categoryRepository.GetCountAsync();

			// Assert
			Assert.True(result);
			Assert.Equal(0, counts.Result);
		}

		[Fact]
		public async void Test_CategoryRepository_Update_Returns_True()
		{
			// Arrange
			Category fakeCategoryToCheck = new Category()
			{
				UserId = "5",
				Name = "Book for test",
				DisplayOrder = 8,
				Genre = "Action",
				ISBN = "433-546-232-765",
				Author = "Badman123",
				Publisher = "Joseph Republic",
				DateTimeNow = DateTime.Now,
				TimeCreated = DateTime.Now,
				TimeUpdated = DateTime.Now,
			};
			var dbContext = await GetDbContext();
			var categoryRepository = new CategoryRepository(dbContext);

			// Act
			categoryRepository.Add(fakeCategoryToCheck);
			var result = categoryRepository.Update(fakeCategoryToCheck);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public async void Test_CategoryRepository_GetIdAsync_Returns_Category()
		{
			// Arrange
			long id = 1;
			var dbContext = await GetDbContext();
			var categoryRepository = new CategoryRepository(dbContext);

			// Act
			var result = categoryRepository.GetIdAsync(id);


			// Assert
			Assert.NotNull(result);
			result.Should().BeOfType<Task<Category>>();
		}

		[Fact]
		public async void Test_CategoryRepository_GetAllCategoryAsync_Returns_List()
		{
			var dbContext = await GetDbContext();
			var categoryRepository = new CategoryRepository(dbContext);

			// Act
			var result = categoryRepository.GetAllCategoriesAsync();


			// Assert
			Assert.NotNull(result);
			result.Should().BeOfType<Task<List<Category>>>();
		}
	}
}
