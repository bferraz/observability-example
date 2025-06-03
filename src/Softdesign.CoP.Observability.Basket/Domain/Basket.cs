namespace Softdesign.CoP.Observability.Basket.Domain
{
    public class Basket
    {
        public Guid Id { get; set; }
        public List<BasketItem> Items { get; set; } = new();
    }
}
