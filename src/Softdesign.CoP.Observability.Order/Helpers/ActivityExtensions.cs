// Helper for OpenTelemetry span enrichment in minimal APIs
using System.Diagnostics;

namespace Softdesign.CoP.Observability.Order.Helpers
{
    public static class ActivityExtensions
    {
        public static void SetTagSafe(this Activity? activity, string key, string? value)
        {
            if (activity != null && value != null)
                activity.SetTag(key, value);
        }
    }
}
