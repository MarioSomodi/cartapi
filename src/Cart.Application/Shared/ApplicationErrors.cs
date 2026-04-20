namespace Cart.Application.Shared;

public static class ApplicationErrors
{
    public static class Auth
    {
        public static readonly Error Unauthenticated =
            new("auth.unauthenticated", "Authentication is required for this operation.");

        public static readonly Error MissingSubjectId =
            new("auth.missing_subject_id", "The authenticated request does not contain a subject identifier.");

        public static readonly Error MissingTenantId =
            new("auth.missing_tenant_id", "The authenticated request does not contain a tenant identifier.");
    }

    public static class Concurrency
    {
        public static readonly Error Conflict =
            new("concurrency.conflict", "The resource was modified by another request. Refresh and try again.");
    }

    public static class Carts
    {
        public static readonly Error CartNotFound =
            new("carts.not_found", "An active cart was not found for the current request context.");

        public static readonly Error ItemNotFound =
            new("carts.item_not_found", "The requested cart item was not found.");

        public static readonly Error InvalidQuantity =
            new("carts.invalid_quantity", "Quantity must be positive.");
    }
}
