
namespace Mss.WorkForce.Code.WFMConnector.Midleware
{
    public sealed class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string HeaderName = "X-API-KEY";
        private readonly IConfiguration _config;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/health") || context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            var expectedKey = _config["X_API_KEY"];

            if (string.IsNullOrWhiteSpace(expectedKey))
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("API Key not configured");
                return;
            }

            if (!context.Request.Headers.TryGetValue(HeaderName, out var receivedKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing API Key");
                return;
            }

            if (!string.Equals(receivedKey, expectedKey, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid API Key");
                return;
            }

            await _next(context);
        }
    }

}
