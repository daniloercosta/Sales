using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Sales.Application.Services;
using Sales.Domain.Interfaces;
using Sales.Infrastructure;
using Sales.Infrastructure.Context;
using Sales.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

/*
 builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseInMemoryDatabase("SalesDb")); // Banco em memória para testes
*/

/*builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseInMemoryDatabase("SalesDb")
           .UseLazyLoadingProxies()  // Se necessário
           .MigrationsAssembly("Sales.Infrastructure")); // Adiciona o assembly das migrations
*/

builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
           //.UseLazyLoadingProxies()  // Se necessário
           //.ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>()  // Força uso do assembly correto
           //.MigrationsAssembly("Sales.Infrastructure")); // Apontando para o assembly de migrations




builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<ISaleService,SaleService>();

builder.Services.AddControllers();
builder.Logging.AddConsole();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
