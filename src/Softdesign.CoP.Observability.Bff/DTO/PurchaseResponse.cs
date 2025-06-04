namespace Softdesign.CoP.Observability.Bff.DTO
{
    public class PurchaseResponse
    {
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalTotal { get; set; }
        public string? Message { get; set; }
    }
}
