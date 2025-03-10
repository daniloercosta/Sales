using Sales.Domain.Entities;

namespace Sales.Domain.Interfaces;

public interface ISaleRepository
{
    Task<Sale> GetByIdAsync(int id);
    Task AddAsync(Sale sale);
    void Update(Sale sale);
    Task SaveChangesAsync();
    Task DeleteAsync(Sale sale);
}