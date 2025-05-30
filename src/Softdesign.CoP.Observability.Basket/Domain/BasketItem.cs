namespace Softdesign.CoP.Observability.Basket.Domain
{
    public class BasketItem
    {        
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Value { get; set; }
        public int Quantity { get; set; }
    }
}
