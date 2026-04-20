namespace Cart.Api.Middleware;

internal static class RequestContextItems
{
    public const string CorrelationId = "CorrelationId";
    public const string CartId = "CartId";

    public static string? GetCorrelationId(this HttpContext httpContext)
    {
        return httpContext.Items.TryGetValue(CorrelationId, out object? value)
            ? value?.ToString()
            : null;
    }

    public static string? GetCartId(this HttpContext httpContext)
    {
        return httpContext.Items.TryGetValue(CartId, out object? value)
            ? value?.ToString()
            : null;
    }

    public static void SetCartId(this HttpContext httpContext, Guid cartId)
    {
        httpContext.Items[CartId] = cartId.ToString();
    }
}
