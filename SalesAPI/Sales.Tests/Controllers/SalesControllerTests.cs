using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Sales.API.Controllers;
using Sales.Application.Services;
using Sales.Domain.Entities;
using Sales.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sales.Tests.Controllers
{
    public class SalesControllerTests
    {
        private readonly Mock<ISaleService> _mockSaleService;
        private readonly SalesController _controller;

        public SalesControllerTests()
        {
            _mockSaleService = new Mock<ISaleService>();
            var mockLogger = new Mock<ILogger<SalesController>>();
            _controller = new SalesController(_mockSaleService.Object, mockLogger.Object);
        }

        [Fact]
        public async Task CreateSale_ShouldReturnCreatedResult_WhenSaleIsCreated()
        {
            var saleNumber = 12345;
            var customer = "Customer 1";
            var branch = "Branch 1";

            var items = new List<SaleItemRequest>
            {
                new SaleItemRequest("Product 1", 2, 100)
            };

            var saleItems = items.Select(i => new SaleItem(i.Product, i.Quantity, i.UnitPrice)).ToList();

            var createSaleRequest = new CreateSaleRequest(saleNumber, customer, branch, items);

            var sale = new Sale(saleNumber, customer, saleItems, 200, DateTime.Now, branch);

            // Configurando os mocks
            _mockSaleService.Setup(s => s.CreateSaleAsync(saleNumber, customer, It.IsAny<List<SaleItem>>(), branch)).ReturnsAsync(1);
            _mockSaleService.Setup(s => s.GetSaleByIdAsync(1)).ReturnsAsync(sale);

            var result = await _controller.CreateSale(createSaleRequest);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);  // Verificando o tipo do resultado
            var returnValue = createdResult.Value as dynamic;  // Pegando o valor retornado pela ação

            // Verificando as propriedades retornadas
            Assert.NotNull(returnValue);
        }

        [Fact]
        public async Task GetSale_ShouldReturnNotFound_WhenSaleDoesNotExist()
        {
            var saleId = 1;
            _mockSaleService.Setup(s => s.GetSaleByIdAsync(saleId)).ReturnsAsync((Sale)null);

            var result = await _controller.GetSale(saleId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CancelSale_ShouldReturnOk_WhenSaleIsCancelled()
        {
            var saleId = 1;
            var sale = new Sale(saleId, "Customer 1", new List<SaleItem>
            {
                new SaleItem("Product 1", 2, 100)
            }, 200, DateTime.Now, "Branch 1");

            _mockSaleService.Setup(s => s.GetSaleByIdAsync(saleId)).ReturnsAsync(sale);
            _mockSaleService.Setup(s => s.CancelSaleAsync(saleId)).Returns(Task.CompletedTask);

            var result = await _controller.CancelSale(saleId);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var json = JsonConvert.SerializeObject(okResult.Value);
            var returnValue = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            Assert.NotNull(returnValue);
            Assert.Equal("Sale cancelled successfully.", returnValue["message"]);

            var logger = new Mock<ILogger<SalesController>>();
            logger.Object.LogInformation("Sale with ID {SaleId} cancelled successfully.", saleId);
        }

        [Fact]
        public async Task CancelItem_ShouldReturnOk_WhenItemIsCancelled()
        {
            var saleId = 1;
            var itemId = 1;
            var sale = new Sale(saleId, "Customer 1", new List<SaleItem>
            {
                new SaleItem("Product 1", 2, 100)
            }, 200, DateTime.Now, "Branch 1");

            _mockSaleService.Setup(s => s.GetSaleByIdAsync(saleId)).ReturnsAsync(sale);
            _mockSaleService.Setup(s => s.CancelItemAsync(saleId, itemId)).Returns(Task.CompletedTask);

            var result = await _controller.CancelItem(saleId, itemId);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var json = JsonConvert.SerializeObject(okResult.Value);
            var returnValue = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            Assert.NotNull(returnValue);
            Assert.Equal("Sale item cancelled successfully.", returnValue["message"]);

            var logger = new Mock<ILogger<SalesController>>();
            logger.Object.LogInformation("Item with ID {ItemId} from Sale ID {SaleId} cancelled successfully.", itemId, saleId);
        }

        [Fact]
        public async Task UpdateItem_ShouldReturnOk_WhenItemIsUpdated()
        {
            var saleId = 1;
            var itemId = 1;
            var request = new UpdateSaleItemRequest("Updated Product", 5, 200);

            _mockSaleService.Setup(s => s.UpdateItemAsync(saleId, itemId, request.NewProduct, request.NewQuantity, request.NewUnitPrice))
                .Returns(Task.CompletedTask);

            var result = await _controller.UpdateItem(saleId, itemId, request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value); // Verifique se o valor dentro do OkObjectResult não é null

            var json = JsonConvert.SerializeObject(okResult.Value);

            var returnValue = JsonConvert.DeserializeObject<JObject>(json);

            // Verifique se a chave 'message' está presente e tem o valor esperado
            Assert.NotNull(returnValue);
            Assert.True(returnValue.ContainsKey("message"));
            Assert.Equal("Sale item updated successfully.", returnValue["message"].ToString());

            var logger = new Mock<ILogger<SalesController>>();
            logger.Object.LogInformation("Item with ID {ItemId} updated in Sale ID {SaleId}.", itemId, saleId);
        }

        [Fact]
        public async Task DeleteSale_ShouldReturnOk_WhenSaleIsDeleted()
        {
            var saleId = 1;
            _mockSaleService.Setup(s => s.DeleteSaleAsync(saleId)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteSale(saleId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value); // Verifique se o valor dentro do OkObjectResult não é null

            var json = JsonConvert.SerializeObject(okResult.Value);

            var returnValue = JsonConvert.DeserializeObject<JObject>(json);

            // Verifique se a chave 'message' está presente e tem o valor esperado
            Assert.NotNull(returnValue);
            Assert.True(returnValue.ContainsKey("message"));
            Assert.Equal("Venda excluída com sucesso.", returnValue["message"].ToString());

            var logger = new Mock<ILogger<SalesController>>();
            logger.Object.LogInformation("Sale with ID {SaleId} deleted successfully.", saleId);
        }

        [Fact]
        public async Task DeleteItem_ShouldReturnOk_WhenItemIsDeleted()
        {
            // Arrange
            var saleId = 1;
            var itemId = 1;

            _mockSaleService.Setup(s => s.DeleteItemAsync(saleId, itemId)).Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILogger<SalesController>>();

            var controller = new SalesController(_mockSaleService.Object, mockLogger.Object);

            var result = await controller.DeleteItem(saleId, itemId);

            var okResult = Assert.IsType<OkObjectResult>(result); // Verifique se o resultado é OkObjectResult
            Assert.NotNull(okResult.Value); // Verifique se o valor dentro do OkObjectResult não é null

            var json = JsonConvert.SerializeObject(okResult.Value);

            var returnValue = JsonConvert.DeserializeObject<JObject>(json);

            Assert.NotNull(returnValue);
            Assert.True(returnValue.ContainsKey("message"));
            Assert.Equal("Item excluído da venda com sucesso.", returnValue["message"].ToString());

            mockLogger.Object.LogInformation("Item with ID {ItemId} deleted from Sale ID {SaleId}.", itemId, saleId);
        }
    }
}
