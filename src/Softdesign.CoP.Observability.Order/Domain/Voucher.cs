namespace Softdesign.CoP.Observability.Order.Domain
{
    public class Voucher
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal Discount { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
