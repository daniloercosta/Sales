using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Interfaces;
using Sales.Infrastructure.Context;

namespace Sales.Infrastructure.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly SalesDbContext _context;

    public SaleRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<Sale> GetByIdAsync(int id)
    {
        return await _context.Sales.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(Sale sale)
    {
        await _context.Sales.AddAsync(sale);
        await _context.SaveChangesAsync();
    }

    public void Update(Sale sale)
    {
        _context.Sales.Update(sale);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Sale sale)
    {
        _context.Sales.Remove(sale);  // Remover a venda
        await _context.SaveChangesAsync();  // Salvar as alterações
    }

}
