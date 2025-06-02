namespace Softdesign.CoP.Observability.Bff.DTO
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Value { get; set; }
        public int QtdStock { get; set; }
    }
}
