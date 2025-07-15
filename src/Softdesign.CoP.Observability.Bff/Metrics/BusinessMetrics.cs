using System.Diagnostics.Metrics;

namespace Softdesign.CoP.Observability.Bff.Metrics
{
    public class BusinessMetrics
    {
        private readonly Meter _meter;
        private readonly Counter<long> _purchaseRequestsTotal;
        private readonly Counter<long> _purchaseSuccessTotal;
        private readonly Counter<long> _purchaseErrorsTotal;

        public BusinessMetrics()
        {
            _meter = new Meter("Softdesign.CoP.Observability.Bff.Business", "1.0.0");

            // Contadores de pedidos
            _purchaseRequestsTotal = _meter.CreateCounter<long>(
                "purchase_requests_total",
                "count",
                "Total number of purchase requests received");

            _purchaseSuccessTotal = _meter.CreateCounter<long>(
                "purchase_success_total",
                "count",
                "Total number of successful purchases");

            _purchaseErrorsTotal = _meter.CreateCounter<long>(
                "purchase_errors_total",
                "count",
                "Total number of failed purchases");
        }

        public void IncrementPurchaseRequests(string correlationId, string userType = "unknown")
        {
            _purchaseRequestsTotal.Add(1,
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("user_type", userType));
        }

        public void IncrementPurchaseSuccess(string correlationId, double valueAmount, int itemCount)
        {
            _purchaseSuccessTotal.Add(1,
                new KeyValuePair<string, object?>("correlation_id", correlationId));
        }

        public void IncrementPurchaseError(string correlationId, string errorType, string errorMessage)
        {
            _purchaseErrorsTotal.Add(1,
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("error_type", errorType),
                new KeyValuePair<string, object?>("error_message", errorMessage));
        }

        public void Dispose()
        {
            _meter?.Dispose();
        }
    }
}
