using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sales.Domain.Entities;
using Sales.Domain.Interfaces;

namespace Sales.API.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;  // Serviço responsável pelas operações de vendas
    private readonly ILogger<SalesController> _logger;  // Logger para registrar informações e erros

    public SalesController(ISaleService saleService, ILogger<SalesController> logger)
    {
        _saleService = saleService;
        _logger = logger;
    }

    // Método para criar uma nova venda
    [HttpPost]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request)
    {
        _logger.LogInformation("Criando venda com número de venda: {SaleNumber}", request.SaleNumber);

        var items = request.Items.Select(i => new SaleItem(i.Product, i.Quantity, i.UnitPrice)).ToList();

        // Chama o serviço para criar a venda
        var saleId = await _saleService.CreateSaleAsync(request.SaleNumber, request.Customer, items, request.Branch);

        _logger.LogInformation("Venda criada com sucesso, ID: {SaleId}", saleId);

        // Obtém os dados da venda recém-criada
        var sale = await _saleService.GetSaleByIdAsync(saleId);

        // Retorna uma resposta com os detalhes da venda criada
        return CreatedAtAction(nameof(GetSale), new { id = saleId }, new
        {
            saleId = sale.Id,
            saleNumber = sale.SaleNumber,
            saleDate = sale.SaleDate,
            customer = sale.Customer,
            totalAmount = sale.TotalAmount,
            branch = sale.Branch,
            isCancelled = sale.IsCancelled,
            items = sale.Items.Select(item => new
            {
                product = item.Product,
                quantity = item.Quantity,
                unitPrice = item.UnitPrice,
                discount = item.Discount,
                isCancelled = item.IsCancelled,
                totalItemAmount = (item.Quantity * item.UnitPrice) - item.Discount
            }).ToList()
        });
    }

    // Método para buscar uma venda pelo ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSale(int id)
    {
        _logger.LogInformation("Buscando venda com ID: {SaleId}", id);

        // Chama o serviço para obter a venda
        var sale = await _saleService.GetSaleByIdAsync(id);

        // Caso a venda não seja encontrada, retorna um erro 404
        if (sale == null)
        {
            _logger.LogWarning("Venda com ID: {SaleId} não encontrada", id);
            return NotFound();
        }

        // Retorna os detalhes da venda
        return Ok(new
        {
            saleId = sale.Id,
            saleNumber = sale.SaleNumber,
            saleDate = sale.SaleDate,
            customer = sale.Customer,
            totalAmount = sale.TotalAmount,
            branch = sale.Branch,
            isCancelled = sale.IsCancelled,
            items = sale.Items.Select(item => new
            {
                product = item.Product,
                quantity = item.Quantity,
                unitPrice = item.UnitPrice,
                discount = item.Discount,
                isCancelled = item.IsCancelled,
                totalItemAmount = (item.Quantity * item.UnitPrice) - item.Discount
            }).ToList()
        });
    }

    // Método para cancelar uma venda
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelSale(int id)
    {
        _logger.LogInformation("Cancelando venda com ID: {SaleId}", id);

        try
        {
            // Chama o serviço para cancelar a venda
            await _saleService.CancelSaleAsync(id);
            _logger.LogInformation("Venda com ID: {SaleId} cancelada com sucesso", id);
            return Ok(new { message = "Venda cancelada com sucesso." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar a venda com ID: {SaleId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    // Método para cancelar um item específico de uma venda
    [HttpPost("{saleId}/items/{itemId}/cancel")]
    public async Task<IActionResult> CancelItem(int saleId, int itemId)
    {
        _logger.LogInformation("Cancelando item com ID: {ItemId} da venda ID: {SaleId}", itemId, saleId);

        try
        {
            // Chama o serviço para cancelar o item da venda
            await _saleService.CancelItemAsync(saleId, itemId);
            _logger.LogInformation("Item com ID: {ItemId} da venda ID: {SaleId} cancelado com sucesso", itemId, saleId);
            return Ok(new { message = "Item da venda cancelado com sucesso." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar item com ID: {ItemId} da venda ID: {SaleId}", itemId, saleId);
            return BadRequest(new { message = ex.Message });
        }
    }

    // Método para atualizar um item de uma venda
    [HttpPut("{saleId}/items/{itemId}")]
    public async Task<IActionResult> UpdateItem(int saleId, int itemId, [FromBody] UpdateSaleItemRequest request)
    {
        _logger.LogInformation("Atualizando item com ID: {ItemId} da venda ID: {SaleId}", itemId, saleId);

        try
        {
            // Chama o serviço para atualizar o item da venda
            await _saleService.UpdateItemAsync(saleId, itemId, request.NewProduct, request.NewQuantity, request.NewUnitPrice);
            _logger.LogInformation("Item com ID: {ItemId} da venda ID: {SaleId} atualizado com sucesso", itemId, saleId);
            return Ok(new { message = "Item da venda atualizado com sucesso." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar item com ID: {ItemId} da venda ID: {SaleId}", itemId, saleId);
            return BadRequest(new { message = ex.Message });
        }
    }

    // Método para excluir uma venda
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSale(int id)
    {
        _logger.LogInformation("Excluindo venda com ID: {SaleId}", id);

        try
        {
            // Chama o serviço para deletar a venda
            await _saleService.DeleteSaleAsync(id);
            _logger.LogInformation("Venda com ID: {SaleId} excluída com sucesso", id);
            return Ok(new { message = "Venda excluída com sucesso." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir a venda com ID: {SaleId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    // Método para excluir um item específico de uma venda
    [HttpDelete("{saleId}/items/{itemId}")]
    public async Task<IActionResult> DeleteItem(int saleId, int itemId)
    {
        _logger.LogInformation("Excluindo item com ID: {ItemId} da venda ID: {SaleId}", itemId, saleId);

        try
        {
            // Chama o serviço para deletar o item da venda
            await _saleService.DeleteItemAsync(saleId, itemId);
            _logger.LogInformation("Item com ID: {ItemId} da venda ID: {SaleId} excluído com sucesso", itemId, saleId);
            return Ok(new { message = "Item excluído da venda com sucesso." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir item com ID: {ItemId} da venda ID: {SaleId}", itemId, saleId);
            return BadRequest(new { message = ex.Message });
        }
    }
}

// Modelos para as requisições e respostas

public record CreateSaleRequest(int SaleNumber, string Customer, string Branch, List<SaleItemRequest> Items);
public record SaleItemRequest(string Product, int Quantity, decimal UnitPrice);
public record UpdateSaleItemRequest(string? NewProduct, int? NewQuantity, decimal? NewUnitPrice);
