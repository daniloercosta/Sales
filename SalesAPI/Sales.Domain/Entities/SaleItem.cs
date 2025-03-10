namespace Sales.Domain.Entities;

public class SaleItem
{
    public int Id { get; private set; }
    public string Product { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Total { get; private set; }
    public bool IsCancelled { get; set; }

    public SaleItem(string product, int quantity, decimal unitPrice)
    {
        if (quantity > 20)
            throw new Exception("Não é possível vender mais de 20 itens do mesmo produto.");

        Id = Id;
        Product = product;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Discount = 0; // Inicialmente sem desconto
        CalculateTotal();
        IsCancelled = false;
    }

    public void ApplyDiscount(decimal discount)
    {
        if (discount < 0)
            throw new ArgumentException("O desconto não pode ser negativo.");

        Discount = discount;
        CalculateTotal();
    }

    public void CancelItem()
    {
        if (IsCancelled) return;

        IsCancelled = true;
        Total = 0; // Se o item for cancelado, seu total é zerado
    }

    private void CalculateTotal()
    {
        Discount = CalculateDiscount();
        Total = (UnitPrice * Quantity) - Discount;
        if (Total < 0) Total = 0;
    }

    public void UpdateProduct(string newProduct)
    {
        Product = newProduct;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity > 20)
            throw new Exception("Não é possível vender mais de 20 itens do mesmo produto.");
        if (newQuantity <= 0)
            throw new Exception("A quantidade deve ser maior que zero.");

        Quantity = newQuantity;
        CalculateTotal();
    }

    public void UpdateUnitPrice(decimal newUnitPrice)
    {
        if (newUnitPrice < 0)
            throw new Exception("O preço unitário não pode ser negativo.");
        UnitPrice = newUnitPrice;
        CalculateTotal();
    }

    private decimal CalculateDiscount()
    {
        if (Quantity <= 4)
            return 0;

        if (Quantity >= 10 && Quantity <= 20)
            return (UnitPrice * Quantity) * 0.20m; // 20% de desconto

        if (Quantity > 4)
            return (UnitPrice * Quantity) * 0.10m; // 10% de desconto

        return 0;
    }
}
