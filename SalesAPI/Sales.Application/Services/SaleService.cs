using Sales.Domain.Entities;
using Sales.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Sales.Application.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly ILogger<SaleService> _logger;

    // Construtor que injeta as dependências do repositório e do logger
    public SaleService(ISaleRepository saleRepository, ILogger<SaleService> logger)
    {
        _saleRepository = saleRepository;
        _logger = logger;
    }

    // Método para criar uma venda
    public async Task<int> CreateSaleAsync(int saleNumber, string customer, List<SaleItem> items, string branch)
    {
        decimal totalAmount = CalculateTotalAmount(items);  // Calcula o total da venda
        var sale = new Sale(saleNumber, customer, items, totalAmount, DateTime.Now, branch);

        _logger.LogInformation("Criando venda {SaleNumber} para o cliente {Customer} no ramo {Branch}.", saleNumber, customer, branch);

        // Persiste a venda no repositório
        await _saleRepository.AddAsync(sale);
        await _saleRepository.SaveChangesAsync();  // Salva as alterações no banco de dados
        _logger.LogInformation("Venda {SaleId} criada com sucesso.", sale.Id);

        return sale.Id;
    }

    // Método para calcular o valor total dos itens da venda
    private decimal CalculateTotalAmount(List<SaleItem> items)
    {
        return items.Sum(item => item.Total);  // Soma o total de cada item
    }

    // Método para buscar uma venda pelo ID
    public async Task<Sale> GetSaleByIdAsync(int saleId)
    {
        _logger.LogInformation("Buscando venda com ID {SaleId}.", saleId);

        var sale = await _saleRepository.GetByIdAsync(saleId);  // Busca a venda no repositório

        if (sale == null)
        {
            _logger.LogWarning("Venda com ID {SaleId} não encontrada.", saleId);
            throw new Exception("Venda não encontrada.");
        }

        return sale;  // Retorna a venda encontrada
    }

    // Método para atualizar uma venda
    public async Task UpdateSaleAsync(Sale sale)
    {
        _logger.LogInformation("Atualizando venda com ID {SaleId}.", sale.Id);

        _saleRepository.Update(sale);  // Atualiza a venda no repositório
        await _saleRepository.SaveChangesAsync();  // Salva as alterações no banco de dados

        _logger.LogInformation("Venda com ID {SaleId} atualizada com sucesso.", sale.Id);
    }

    // Método para cancelar uma venda
    public async Task CancelSaleAsync(int saleId)
    {
        _logger.LogInformation("Cancelando venda com ID {SaleId}.", saleId); 

        var sale = await _saleRepository.GetByIdAsync(saleId);  // Busca a venda pelo ID
        if (sale == null)
        {
            _logger.LogWarning("Venda com ID {SaleId} não encontrada para cancelamento.", saleId);  
            throw new Exception("Venda não encontrada.");
        }

        sale.CancelSale();  // Cancela a venda
        _saleRepository.Update(sale);  // Atualiza a venda no repositório
        await _saleRepository.SaveChangesAsync();  // Salva as alterações no banco de dados

        _logger.LogInformation("Venda com ID {SaleId} cancelada com sucesso.", saleId);  
    }

    // Método para cancelar um item de uma venda
    public async Task CancelItemAsync(int saleId, int itemId)
    {
        _logger.LogInformation("Cancelando item com ID {ItemId} na venda com ID {SaleId}.", itemId, saleId);  

        var sale = await _saleRepository.GetByIdAsync(saleId);  // Busca a venda pelo ID
        if (sale == null)
        {
            _logger.LogWarning("Venda com ID {SaleId} não encontrada para cancelamento do item.", saleId);  
            throw new Exception("Venda não encontrada.");
        }

        sale.CancelItem(itemId);  // Cancela o item da venda
        _saleRepository.Update(sale);  // Atualiza a venda no repositório
        await _saleRepository.SaveChangesAsync();  // Salva as alterações no banco de dados

        _logger.LogInformation("Item com ID {ItemId} na venda com ID {SaleId} cancelado com sucesso.", itemId, saleId);  
    }

    // Método para atualizar um item de uma venda
    public async Task UpdateItemAsync(int saleId, int itemId, string? newProduct, int? newQuantity, decimal? newUnitPrice)
    {
        _logger.LogInformation("Atualizando item com ID {ItemId} na venda com ID {SaleId}.", itemId, saleId);  

        var sale = await _saleRepository.GetByIdAsync(saleId);  // Busca a venda pelo ID
        if (sale == null)
        {
            _logger.LogWarning("Venda com ID {SaleId} não encontrada para atualização do item.", saleId);  
            throw new Exception("Venda não encontrada.");
        }

        sale.UpdateItem(itemId, newProduct, newQuantity, newUnitPrice);  // Atualiza o item da venda
        _saleRepository.Update(sale);  // Atualiza a venda no repositório
        await _saleRepository.SaveChangesAsync();  // Salva as alterações no banco de dados

        _logger.LogInformation("Item com ID {ItemId} na venda com ID {SaleId} atualizado com sucesso.", itemId, saleId);
    }

    // Método para excluir uma venda
    public async Task DeleteSaleAsync(int saleId)
    {
        _logger.LogInformation("Excluindo venda com ID {SaleId}.", saleId);  

        var sale = await _saleRepository.GetByIdAsync(saleId);  // Busca a venda pelo ID
        if (sale == null)
        {
            _logger.LogWarning("Venda com ID {SaleId} não encontrada para exclusão.", saleId);
            throw new Exception("Venda não encontrada.");
        }

        // Exclui a venda
        await _saleRepository.DeleteAsync(sale);
        await _saleRepository.SaveChangesAsync();  // Salva as alterações no banco de dados

        _logger.LogInformation("Venda com ID {SaleId} excluída com sucesso.", saleId);  
    }

    // Método para excluir um item específico de uma venda
    public async Task DeleteItemAsync(int saleId, int itemId)
    {
        _logger.LogInformation("Excluindo item com ID {ItemId} da venda com ID {SaleId}.", itemId, saleId);

        var sale = await _saleRepository.GetByIdAsync(saleId);  // Busca a venda pelo ID
        if (sale == null)
        {
            _logger.LogWarning("Venda com ID {SaleId} não encontrada para exclusão do item.", saleId);  
            throw new Exception("Venda não encontrada.");
        }

        var item = sale.Items.FirstOrDefault(i => i.Id == itemId);  // Busca o item na venda
        if (item == null)
        {
            _logger.LogWarning("Item com ID {ItemId} não encontrado na venda com ID {SaleId}.", itemId, saleId);  
            throw new Exception("Item não encontrado.");
        }

        // Remove o item da venda
        sale.Items.Remove(item);

        // Atualiza a venda (o total será recalculado automaticamente)
        _saleRepository.Update(sale);
        await _saleRepository.SaveChangesAsync();  // Salva as alterações no banco de dados

        _logger.LogInformation("Item com ID {ItemId} excluído da venda com ID {SaleId} com sucesso.", itemId, saleId);  
    }
}
