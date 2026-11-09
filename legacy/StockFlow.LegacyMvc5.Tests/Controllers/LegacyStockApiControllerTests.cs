using System;
using System.Net;
using System.Web.Http.Results;
using Moq;
using StockFlow.LegacyMvc5.Controllers.Api;
using StockFlow.LegacyMvc5.Data;
using StockFlow.LegacyMvc5.Models;
using Xunit;

namespace StockFlow.LegacyMvc5.Tests.Controllers
{
    public class LegacyStockApiControllerTests
    {
        private readonly Mock<ILegacyStockRepository> _repositoryMock;
        private readonly LegacyStockApiController _controller;

        public LegacyStockApiControllerTests()
        {
            _repositoryMock = new Mock<ILegacyStockRepository>();
            _controller = new LegacyStockApiController(_repositoryMock.Object);
        }

        [Fact]
        public void Get_WhenProductExists_ReturnsOkWithProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var expectedProduct = new LegacyProduct
            {
                Id = productId,
                Name = "Dell Latitude 5550",
                SKU = "DL-5550",
                QuantityInStock = 15
            };
            _repositoryMock.Setup(r => r.GetById(productId)).Returns(expectedProduct);

            // Act
            var actionResult = _controller.Get(productId);

            // Assert
            var okResult = Assert.IsType<OkNegotiatedContentResult<LegacyProduct>>(actionResult);
            Assert.NotNull(okResult.Content);
            Assert.Equal(productId, okResult.Content.Id);
            Assert.Equal("DL-5550", okResult.Content.SKU);
        }

        [Fact]
        public void Get_WhenProductNotFound_ReturnsNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetById(productId)).Returns((LegacyProduct)null);

            // Act
            var actionResult = _controller.Get(productId);

            // Assert
            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public void Adjust_WhenValidRequest_ReturnsOkWithUpdatedStock()
        {
            // Arrange
            var productId = Guid.NewGuid();
            int newQty = 25;
            string error;
            _repositoryMock.Setup(r => r.AdjustStock(productId, 10, out newQty, out error))
                .Returns(true);

            var request = new LegacyStockAdjustmentRequest
            {
                ProductId = productId,
                Delta = 10
            };

            // Act
            var actionResult = _controller.Adjust(request);

            // Assert
            var okResult = Assert.IsType<OkNegotiatedContentResult<LegacyStockAdjustmentResponse>>(actionResult);
            Assert.Equal(productId, okResult.Content.ProductId);
            Assert.Equal(25, okResult.Content.NewQuantity);
            Assert.Equal(10, okResult.Content.Delta);
        }

        [Fact]
        public void Adjust_WhenDeltaIsZero_ReturnsBadRequest()
        {
            // Arrange
            var request = new LegacyStockAdjustmentRequest
            {
                ProductId = Guid.NewGuid(),
                Delta = 0
            };

            // Act
            var actionResult = _controller.Adjust(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestErrorMessageResult>(actionResult);
            Assert.Equal("Adjustment delta cannot be zero.", badRequest.Message);
        }

        [Fact]
        public void Adjust_WhenProductNotFound_ReturnsNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            int newQty = 0;
            string error = "product not found";
            _repositoryMock.Setup(r => r.AdjustStock(productId, 5, out newQty, out error))
                .Returns(false);

            var request = new LegacyStockAdjustmentRequest
            {
                ProductId = productId,
                Delta = 5
            };

            // Act
            var actionResult = _controller.Adjust(request);

            // Assert
            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public void Adjust_WhenStockWouldGoNegative_ReturnsConflict409()
        {
            // Arrange
            var productId = Guid.NewGuid();
            int currentQty = 5;
            string error = "stock would go negative";
            _repositoryMock.Setup(r => r.AdjustStock(productId, -10, out currentQty, out error))
                .Returns(false);

            var request = new LegacyStockAdjustmentRequest
            {
                ProductId = productId,
                Delta = -10
            };

            // Act
            var actionResult = _controller.Adjust(request);

            // Assert
            var contentResult = Assert.IsType<NegotiatedContentResult<LegacyStockAdjustmentResponse>>(actionResult);
            Assert.Equal(HttpStatusCode.Conflict, contentResult.StatusCode);
            Assert.Equal("stock would go negative", contentResult.Content.Message);
        }
    }
}
