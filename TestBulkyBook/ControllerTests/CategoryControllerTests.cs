using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TestBulkyBook.CategoryTest
{
	public class CategoryControllerTests
	{
		[Fact]
		public void Test_CategoryController_Index_Action_Returns_View()
		{
			var controller = new CategoryController();
			//CategoryController controller.Index() as ViewResult;

		}
	}
}
