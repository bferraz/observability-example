namespace Softdesign.CoP.Observability.Order.Domain
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
        public int QtdStock { get; set; }
    }
}
