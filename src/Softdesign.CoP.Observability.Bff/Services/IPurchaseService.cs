using Softdesign.CoP.Observability.Bff.DTO;
using Softdesign.CoP.Observability.Bff.Requests;

namespace Softdesign.CoP.Observability.Bff.Services
{
    public interface IPurchaseService
    {
        Task<(bool Success, PurchaseResponse? Response, string? ErrorMessage)> ProcessPurchaseAsync(PurchaseRequest request);
    }
}
