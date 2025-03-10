namespace Sales.Domain.Entities
{
    public class Sale
    {
        public int Id { get; private set; }
        public int SaleNumber { get; private set; }
        public DateTime SaleDate { get; private set; }
        public string Customer { get; private set; }
        public decimal TotalAmount => Items.Sum(item => item.Total); // Total geral da venda com descontos
        public string Branch { get; private set; } // Filial onde a venda foi realizada
        public bool IsCancelled { get; private set; }
        public List<SaleItem> Items { get; private set; } = new();

        public Sale() { }

        public Sale(int saleNumber, string customer, List<SaleItem> items, decimal totalAmount, DateTime saleDate, string branch)
        {
            SaleNumber = saleNumber;
            Customer = customer;
            Items = items;
            SaleDate = saleDate; 
            Branch = branch ?? throw new ArgumentNullException(nameof(branch)); // <- Evita valores nulos
        }

        public void SetSaleDate(DateTime saleDate)
        {
            SaleDate = saleDate;
        }

        public void CancelSale()
        {
            if (IsCancelled) return; // Evita cancelamento repetido

            IsCancelled = true;
            foreach (var item in Items)
            {
                item.CancelItem();
            }
        }

        public void CancelItem(int itemId)
        {
            var item = Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new Exception("Item not found.");
            if (item != null)
            {
                item.CancelItem();
                if (Items.All(i => i.IsCancelled))
                {
                    IsCancelled = true; // Se todos os itens foram cancelados, a venda também é cancelada
                }
            }
        }

        public void ApplyDiscounts()
        {
            foreach (var item in Items)
            {
                decimal discount = 0;
                if (item.Quantity >= 10)
                {
                    discount = item.UnitPrice * item.Quantity * 0.2m;
                }
                else if (item.Quantity >= 4)
                {
                    discount = item.UnitPrice * item.Quantity * 0.1m;
                }
                item.ApplyDiscount(discount);
            }
        }

        public void UpdateItem(int itemId, string? newProduct, int? newQuantity, decimal? newUnitPrice)
        {
            var item = Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new Exception("Item not found.");

            if (!string.IsNullOrEmpty(newProduct))
                item.UpdateProduct(newProduct);

            if (newQuantity.HasValue)
                item.UpdateQuantity(newQuantity.Value);

            if (newUnitPrice.HasValue)
                item.UpdateUnitPrice(newUnitPrice.Value);
        }
    }
}