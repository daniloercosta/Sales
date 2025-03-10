# Sales
 Api de crud de vendas e itens e testes de controllers e services

Na raiz do projeto
rodar esse comando
dotnet ef migrations add InitialCreate1 --project Sales.Infrastructure --startup-project Sales.API
e depois esse
dotnet ef database update --project Sales.Infrastructure --startup-project Sales.API

Para o banco de dados ser criado.
após isso rodar a api, segue um exemplo de json para ser criada a venda:

{
  "saleNumber": 1,
  "customer": "John Doe",
  "items": [
    {
      "product": "Product 1",
      "quantity": 2,
      "unitPrice": 100.00,
      "discount": 10.00
    },
    {
      "product": "Product 2",
      "quantity": 3,
      "unitPrice": 50.00,
      "discount": 5.00
    }
  ],
  "branch": "Branch 1"
}
