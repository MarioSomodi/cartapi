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
}
