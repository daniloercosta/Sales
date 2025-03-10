using Microsoft.Extensions.Logging;
using Moq;
using Sales.API.Controllers;
using Sales.Application.Services;
using Sales.Domain.Entities;
using Sales.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sales.Tests.Services
{
    public class SaleServiceTests
    {
        private readonly Mock<ISaleRepository> _mockSaleRepository;
        private readonly Mock<ILogger<SaleService>> _mockLogger;
        private readonly SaleService _saleService;

        public SaleServiceTests()
        {
            _mockSaleRepository = new Mock<ISaleRepository>();
            _mockLogger = new Mock<ILogger<SaleService>>();
            _saleService = new SaleService(_mockSaleRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateSaleAsync_ShouldReturnSaleId_WhenSaleIsCreated()
        {
            var saleNumber = 1;
            var customer = "John Doe";
            var items = new List<SaleItem>
            {
                new SaleItem("Product 1", 2, 100m)
            };
            var branch = "Branch 1";
            var saleId = 1;

            var sale = new Sale(saleNumber, customer, items, 200m, DateTime.Now, branch);

            _mockSaleRepository.Setup(r => r.AddAsync(It.IsAny<Sale>()))
                .Callback<Sale>(s => s.GetType().GetProperty("Id").SetValue(s, saleId)) // Garantindo que o ID seja setado no objeto Sale
                .Returns(Task.CompletedTask);

            _mockSaleRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _saleService.CreateSaleAsync(saleNumber, customer, items, branch);

            Assert.Equal(saleId, result); // Verifica se o id da venda é retornado corretamente
            _mockSaleRepository.Verify(r => r.AddAsync(It.IsAny<Sale>()), Times.Once); // Verifica se o método AddAsync foi chamado
            _mockSaleRepository.Verify(r => r.SaveChangesAsync(), Times.Once); // Verifica se o SaveChangesAsync foi chamado

            _mockLogger.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetSaleByIdAsync_ShouldReturnSale_WhenSaleExists()
        {
            var saleId = 0;
            var saleNumber = 1; // Adicionando o parâmetro saleNumber
            var customer = "John Doe";
            var items = new List<SaleItem>();
            var totalAmount = 100m;
            var date = DateTime.Now;
            var branch = "Branch 1";

            var sale = new Sale(saleNumber, customer, items, totalAmount, date, branch);

            _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId)).ReturnsAsync(sale);

            var result = await _saleService.GetSaleByIdAsync(saleId);

            Console.WriteLine($"Expected Sale Id: {saleId}, Actual Sale Id: {result?.Id}");

            Assert.NotNull(result); // Verifica se o resultado não é nulo
            Assert.Equal(saleId, result.Id); // Verifica se o ID da venda retornada é o esperado

            _mockLogger.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task UpdateSaleAsync_ShouldCallUpdate_WhenSaleIsUpdated()
        {
            var sale = new Sale(1, "John Doe", new List<SaleItem>(), 100, DateTime.Now, "Branch 1");

            _mockSaleRepository.Setup(r => r.Update(It.IsAny<Sale>()));

            await _saleService.UpdateSaleAsync(sale);

            _mockSaleRepository.Verify(r => r.Update(It.IsAny<Sale>()), Times.Once); // Verifica se o método Update foi chamado
            _mockSaleRepository.Verify(r => r.SaveChangesAsync(), Times.Once); // Verifica se o SaveChangesAsync foi chamado

            _mockLogger.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task CancelSaleAsync_ShouldThrowException_WhenSaleNotFound()
        {
            var saleId = 1;
            _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId)).ReturnsAsync((Sale)null);

            var exception = await Assert.ThrowsAsync<Exception>(() => _saleService.CancelSaleAsync(saleId));
            Assert.Equal("Sale not found.", exception.Message);

            _mockLogger.Verify(logger => logger.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task CancelItemAsync_ShouldThrowException_WhenItemNotFound()
        {
            var saleId = 1;
            var itemId = 999; // Um item que não existe
            var sale = new Sale(saleId, "Cliente Teste", new List<SaleItem>(), 100, DateTime.Now, "Filial A");

            _mockSaleRepository
                .Setup(repo => repo.GetByIdAsync(saleId))
                .ReturnsAsync(sale); // Retorna uma venda válida, mas sem itens

            var exception = await Assert.ThrowsAsync<Exception>(() => _saleService.CancelItemAsync(saleId, itemId));
            Assert.Equal("Item not found.", exception.Message);

            _mockLogger.Verify(logger => logger.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task DeleteSaleAsync_ShouldThrowException_WhenSaleNotFound()
        {
            var saleId = 1;
            _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId)).ReturnsAsync((Sale)null);

            var exception = await Assert.ThrowsAsync<Exception>(() => _saleService.DeleteSaleAsync(saleId));
            Assert.Equal("Sale not found.", exception.Message);

            _mockLogger.Verify(logger => logger.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task DeleteItemAsync_ShouldThrowException_WhenItemNotFound()
        {
            var saleId = 1;
            var itemId = 1;
            var sale = new Sale(saleId, "John Doe", new List<SaleItem>(), 100, DateTime.Now, "Branch 1");

            _mockSaleRepository.Setup(r => r.GetByIdAsync(saleId)).ReturnsAsync(sale);

            var exception = await Assert.ThrowsAsync<Exception>(() => _saleService.DeleteItemAsync(saleId, itemId));
            Assert.Equal("Item not found.", exception.Message);

            _mockLogger.Verify(logger => logger.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
    }
}
