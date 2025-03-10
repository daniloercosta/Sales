using Sales.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sales.Domain.Interfaces
{
    public interface ISaleService
    {
        Task<int> CreateSaleAsync(int saleNumber, string customer, List<SaleItem> items, string branch);
        Task<Sale> GetSaleByIdAsync(int saleId);
        Task CancelSaleAsync(int saleId);
        Task CancelItemAsync(int saleId, int itemId);
        Task UpdateItemAsync(int saleId, int itemId, string newProduct, int? newQuantity, decimal? newUnitPrice);
        Task DeleteSaleAsync(int saleId);
        Task DeleteItemAsync(int saleId, int itemId);
    }

}
