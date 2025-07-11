using CorrelationId.Abstractions;

namespace Softdesign.CoP.Observability.Bff.Helpers
{
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        public CorrelationIdDelegatingHandler(ICorrelationContextAccessor correlationContextAccessor)
        {
            _correlationContextAccessor = correlationContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationId = _correlationContextAccessor.CorrelationContext?.CorrelationId;

            if (!string.IsNullOrEmpty(correlationId))
            {
                request.Headers.Add("X-Correlation-ID", correlationId);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
