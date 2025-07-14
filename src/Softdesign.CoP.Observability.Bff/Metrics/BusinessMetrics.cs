using System.Diagnostics.Metrics;

namespace Softdesign.CoP.Observability.Bff.Metrics
{
    public class BusinessMetrics
    {
        private readonly Meter _meter;
        private readonly Counter<long> _purchaseRequestsTotal;
        private readonly Counter<long> _purchaseSuccessTotal;
        private readonly Counter<long> _purchaseErrorsTotal;
        private readonly Histogram<double> _purchaseDuration;
        private readonly Counter<long> _basketOperationsTotal;
        private readonly Counter<long> _orderOperationsTotal;
        private readonly Histogram<double> _purchaseValue;

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

            // Histograma de duração de pedidos
            _purchaseDuration = _meter.CreateHistogram<double>(
                "purchase_duration_seconds",
                "seconds",
                "Duration of purchase operations");

            // Contadores de operações em serviços downstream
            _basketOperationsTotal = _meter.CreateCounter<long>(
                "basket_operations_total",
                "count",
                "Total number of basket service operations");

            _orderOperationsTotal = _meter.CreateCounter<long>(
                "order_operations_total",
                "count",
                "Total number of order service operations");

            // Histograma de valor dos pedidos
            _purchaseValue = _meter.CreateHistogram<double>(
                "purchase_value_amount",
                "currency",
                "Value amount of purchases");
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

            _purchaseValue.Record(valueAmount,
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("item_count", itemCount));
        }

        public void IncrementPurchaseError(string correlationId, string errorType, string errorMessage)
        {
            _purchaseErrorsTotal.Add(1,
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("error_type", errorType),
                new KeyValuePair<string, object?>("error_message", errorMessage));
        }

        public void RecordPurchaseDuration(double durationSeconds, string correlationId, string status)
        {
            _purchaseDuration.Record(durationSeconds,
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("status", status));
        }

        public void IncrementBasketOperations(string operation, string correlationId, bool success = true)
        {
            _basketOperationsTotal.Add(1,
                new KeyValuePair<string, object?>("operation", operation),
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("success", success));
        }

        public void IncrementOrderOperations(string operation, string correlationId, bool success = true)
        {
            _orderOperationsTotal.Add(1,
                new KeyValuePair<string, object?>("operation", operation),
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("success", success));
        }

        public void Dispose()
        {
            _meter?.Dispose();
        }
    }
}
