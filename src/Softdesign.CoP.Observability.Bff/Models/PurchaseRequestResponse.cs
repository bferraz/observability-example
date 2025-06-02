namespace Softdesign.CoP.Observability.Bff.Requests
{
    public class PurchaseRequest
    {
        public string? VoucherCode { get; set; }
    }
}

namespace Softdesign.CoP.Observability.Bff.Responses
{
    public class PurchaseResponse
    {
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalTotal { get; set; }
        public string? Message { get; set; }
    }
}
