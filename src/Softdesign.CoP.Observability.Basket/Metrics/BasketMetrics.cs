using System.Diagnostics.Metrics;

namespace Softdesign.CoP.Observability.Basket.Metrics
{
    public class BasketMetrics
    {
        private readonly Meter _meter;
        private readonly Counter<long> _basketOperationsTotal;
        private readonly Counter<long> _basketItemsTotal;
        private readonly Histogram<double> _basketValue;
        private readonly Histogram<double> _basketOperationDuration;

        public BasketMetrics()
        {
            _meter = new Meter("Softdesign.CoP.Observability.Basket.Business", "1.0.0");

            _basketOperationsTotal = _meter.CreateCounter<long>(
                "basket_operations_total",
                "count",
                "Total number of basket operations");

            _basketItemsTotal = _meter.CreateCounter<long>(
                "basket_items_total",
                "count",
                "Total number of items in baskets");

            _basketValue = _meter.CreateHistogram<double>(
                "basket_value_amount",
                "currency",
                "Value amount of baskets");

            _basketOperationDuration = _meter.CreateHistogram<double>(
                "basket_operation_duration_seconds",
                "seconds",
                "Duration of basket operations");
        }

        public void IncrementBasketOperations(string correlationId, string operation, bool success = true)
        {
            _basketOperationsTotal.Add(1,
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("operation", operation),
                new KeyValuePair<string, object?>("success", success));
        }

        public void IncrementBasketItems(string correlationId, int itemCount, double totalValue)
        {
            _basketItemsTotal.Add(itemCount,
                new KeyValuePair<string, object?>("correlation_id", correlationId));

            _basketValue.Record(totalValue,
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("item_count", itemCount));
        }

        public void RecordOperationDuration(double durationSeconds, string correlationId, string operation)
        {
            _basketOperationDuration.Record(durationSeconds,
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("operation", operation));
        }

        public void Dispose()
        {
            _meter?.Dispose();
        }
    }
}
