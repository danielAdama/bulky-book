using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FakeItEasy;
using BulkyBookWeb.Controllers;
using System.Threading.Tasks;
using FluentAssertions;
using BulkyBookWeb.Data;
using Microsoft.AspNetCore.Mvc;
using BulkyBookWeb.Models;
using Microsoft.EntityFrameworkCore;
using BulkyBookWeb.Repository;
using BulkyBookWeb.Interfaces;

namespace BulkyBookWeb.Tests.ControllerTests
{
	public class CategoryControllerTests
	{
		private CategoryController _categoryController;
		private ICategoryRepository _categoryRepository;

		public CategoryControllerTests()
		{
			// Dependencies
			_categoryRepository = A.Fake<ICategoryRepository>();

			// SUT(System Under Test)
			_categoryController = new CategoryController(_categoryRepository);
		}

		[Fact]
		public void Test_CategoryController_Index_Returns_IActionResultType()
		{
			// Arrange - What do i need to bring in?
			var objCategoryList = A.Fake<List<Category>>();
			A.CallTo(() => _categoryRepository.GetAllCategoriesAsync()).Returns(objCategoryList);
			// Act
			var result = _categoryController.Index();
			// Assert - Object check actions
			result.Should().BeOfType<Task<IActionResult>>();
		}

		[Fact]
		public void Test_CategoryController_Create_Returns_View()
		{
			// Arrange & Act
			var result = _categoryController.Create();
			// Assert
			result.Should().BeOfType<ViewResult>();
			Assert.Null(result.ViewName);
		}

		[Fact]
		public void Test_CategoryController_Edit_Returns_IActionResult()
		{
			// Arrange
			long id = 1;
			Category bookCategory = A.Fake<Category>();
			A.CallTo(() => _categoryRepository.GetIdAsync(id)).Returns(bookCategory);
			// Act
			var result = _categoryController.Edit(id);
			result.Should().BeOfType<Task<IActionResult>>();
		}

		[Fact]
		public void Test_CategoryController_Delete_Returns_IActionResult()
		{
			// Arrange
			long id = 1;
			Category bookCategory = A.Fake<Category>();
			A.CallTo(() => _categoryRepository.GetIdAsync(id)).Returns(bookCategory);
			// Act
			var result = _categoryController.Delete(id) ;
			result.Should().BeOfType<Task<IActionResult>>();
		}

	}
}
