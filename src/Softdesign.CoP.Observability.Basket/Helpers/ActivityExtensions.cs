using System.Diagnostics;

namespace Softdesign.CoP.Observability.Basket.Helpers
{
    public static class ActivityExtensions
    {
        public static void SetTagSafe(this Activity? activity, string key, string? value)
        {
            if (activity != null && value != null)
            {
                activity.SetTag(key, value);
            }
        }
    }
}
