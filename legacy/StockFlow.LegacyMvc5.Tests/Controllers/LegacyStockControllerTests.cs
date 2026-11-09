using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Moq;
using StockFlow.LegacyMvc5.Controllers;
using StockFlow.LegacyMvc5.Data;
using StockFlow.LegacyMvc5.Models;
using Xunit;

namespace StockFlow.LegacyMvc5.Tests.Controllers
{
    public class LegacyStockControllerTests
    {
        private readonly Mock<ILegacyStockRepository> _repositoryMock;
        private readonly StockController _controller;

        public LegacyStockControllerTests()
        {
            _repositoryMock = new Mock<ILegacyStockRepository>();
            _controller = new StockController(_repositoryMock.Object);
        }

        [Fact]
        public void Index_ReturnsViewResultWithAllProducts()
        {
            // Arrange
            var products = new List<LegacyProduct>
            {
                new LegacyProduct { Id = Guid.NewGuid(), Name = "Wireless Mouse", SKU = "WM-01", QuantityInStock = 20 },
                new LegacyProduct { Id = Guid.NewGuid(), Name = "Mechanical Keyboard", SKU = "KB-01", QuantityInStock = 8 }
            };
            _repositoryMock.Setup(r => r.GetAll()).Returns(products);

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<LegacyProduct>>(result.Model);
            Assert.Equal(2, model.Count());
            Assert.Equal(".NET Framework 4.8 / ASP.NET MVC 5", result.ViewBag.FrameworkVersion);
        }

        [Fact]
        public void Adjust_WhenModelInvalid_ReturnsViewWithErrorMessage()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAll()).Returns(new List<LegacyProduct>());
            _controller.ModelState.AddModelError("Delta", "Delta is required");

            var request = new LegacyStockAdjustmentRequest();

            // Act
            var result = _controller.Adjust(request) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ViewName);
            Assert.Contains("Validation failed", (string)result.ViewBag.ErrorMessage);
        }

        [Fact]
        public void Adjust_WhenAdjustmentSuccessful_SetsTempDataAndRedirectsToIndex()
        {
            // Arrange
            var productId = Guid.NewGuid();
            int newQty = 30;
            string error;
            _repositoryMock.Setup(r => r.AdjustStock(productId, 10, out newQty, out error)).Returns(true);

            var request = new LegacyStockAdjustmentRequest
            {
                ProductId = productId,
                Delta = 10
            };

            // Act
            var result = _controller.Adjust(request) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.RouteValues["action"]);
            Assert.Contains("Product stock adjusted successfully to 30", (string)_controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public void Adjust_WhenAdjustmentFails_ReturnsViewWithErrorMessage()
        {
            // Arrange
            var productId = Guid.NewGuid();
            int newQty = 0;
            string error = "stock would go negative";
            _repositoryMock.Setup(r => r.AdjustStock(productId, -50, out newQty, out error)).Returns(false);
            _repositoryMock.Setup(r => r.GetAll()).Returns(new List<LegacyProduct>());

            var request = new LegacyStockAdjustmentRequest
            {
                ProductId = productId,
                Delta = -50
            };

            // Act
            var result = _controller.Adjust(request) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ViewName);
            Assert.Equal("stock would go negative", (string)result.ViewBag.ErrorMessage);
        }
    }
}
