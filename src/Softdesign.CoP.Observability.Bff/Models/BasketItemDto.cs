namespace Softdesign.CoP.Observability.Bff.Models
{
    public class BasketItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Value { get; set; }
        public int Quantity { get; set; }
    }
}
