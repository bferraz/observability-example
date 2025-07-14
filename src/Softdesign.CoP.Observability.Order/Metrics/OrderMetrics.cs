using System.Diagnostics.Metrics;

namespace Softdesign.CoP.Observability.Order.Metrics
{
    public class OrderMetrics
    {
        private readonly Meter _meter;
        private readonly Counter<long> _productQueriesTotal;
        private readonly Counter<long> _productUpdatesTotal;
        private readonly Counter<long> _voucherOperationsTotal;
        private readonly Histogram<double> _productQueryDuration;
        private readonly Counter<long> _stockOperationsTotal;

        public OrderMetrics()
        {
            _meter = new Meter("Softdesign.CoP.Observability.Order.Business", "1.0.0");

            _productQueriesTotal = _meter.CreateCounter<long>(
                "order_product_queries_total",
                "count",
                "Total number of product queries");

            _productUpdatesTotal = _meter.CreateCounter<long>(
                "order_product_updates_total",
                "count",
                "Total number of product updates");

            _voucherOperationsTotal = _meter.CreateCounter<long>(
                "order_voucher_operations_total",
                "count",
                "Total number of voucher operations");

            _productQueryDuration = _meter.CreateHistogram<double>(
                "order_product_query_duration_seconds",
                "seconds",
                "Duration of product query operations");

            _stockOperationsTotal = _meter.CreateCounter<long>(
                "order_stock_operations_total",
                "count",
                "Total number of stock operations");
        }

        public void IncrementProductQueries(string correlationId, string operation, bool success = true)
        {
            _productQueriesTotal.Add(1,
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("operation", operation),
                new KeyValuePair<string, object?>("success", success));
        }

        public void IncrementProductUpdates(string correlationId, int stockReduced, bool success = true)
        {
            _productUpdatesTotal.Add(1,
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("stock_reduced", stockReduced),
                new KeyValuePair<string, object?>("success", success));
        }

        public void IncrementVoucherOperations(string correlationId, string operation, bool success = true)
        {
            _voucherOperationsTotal.Add(1,
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("operation", operation),
                new KeyValuePair<string, object?>("success", success));
        }

        public void RecordProductQueryDuration(double durationSeconds, string correlationId, string operation)
        {
            _productQueryDuration.Record(durationSeconds,
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("operation", operation));
        }

        public void IncrementStockOperations(string correlationId, string operation, int quantity, bool success = true)
        {
            _stockOperationsTotal.Add(1,
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("operation", operation),
                new KeyValuePair<string, object?>("quantity", quantity),
                new KeyValuePair<string, object?>("success", success));
        }

        public void Dispose()
        {
            _meter?.Dispose();
        }
    }
}
