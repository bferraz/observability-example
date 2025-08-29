using System.Diagnostics.Metrics;

namespace Softdesign.CoP.Observability.Catalog.Metrics
{
    public class CatalogMetrics
    {
        private readonly Meter _meter;
        private readonly Counter<long> _productQueriesTotal;
        private readonly Counter<long> _stockOperationsTotal;

        public CatalogMetrics()
        {
            _meter = new Meter("Softdesign.CoP.Observability.Catalog.Business", "1.0.0");

            _productQueriesTotal = _meter.CreateCounter<long>(
                "catalog_product_queries_total",
                description: "Total number of product queries");

            _stockOperationsTotal = _meter.CreateCounter<long>(
                "catalog_stock_operations_total", 
                description: "Total number of stock operations");
        }

        public void IncrementProductQueries(string operation, string correlationId, bool success)
        {
            _productQueriesTotal.Add(1,
                new KeyValuePair<string, object?>("operation", operation),
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("success", success.ToString()));
        }

        public void IncrementStockOperations(string operation, string correlationId, bool success)
        {
            _stockOperationsTotal.Add(1,
                new KeyValuePair<string, object?>("operation", operation),
                new KeyValuePair<string, object?>("correlation_id", correlationId),
                new KeyValuePair<string, object?>("success", success.ToString()));
        }

        public void Dispose()
        {
            _meter?.Dispose();
        }
    }
}