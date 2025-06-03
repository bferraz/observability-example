namespace Softdesign.CoP.Observability.Bff.Requests
{
    public class PurchaseRequest
    {
        public Guid UserId { get; set; }
        public string? VoucherCode { get; set; }
    }
}
