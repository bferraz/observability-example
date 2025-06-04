namespace Softdesign.CoP.Observability.Bff.DTO
{
    public class BasketDto
    {
        public Guid Id { get; set; }
        public List<BasketItemDto> Items { get; set; } = new();
    }
}
