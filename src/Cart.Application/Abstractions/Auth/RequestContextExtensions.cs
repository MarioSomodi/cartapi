using Cart.Application.Shared;

namespace Cart.Application.Abstractions.Auth;

public static class RequestContextExtensions
{
    public static Result<RequestIdentity> GetRequiredIdentity(this IRequestContext requestContext)
    {
        ArgumentNullException.ThrowIfNull(requestContext);

        if (!requestContext.IsAuthenticated)
        {
            return Result<RequestIdentity>.Failure(ApplicationErrors.Auth.Unauthenticated);
        }

        if (string.IsNullOrWhiteSpace(requestContext.SubjectId))
        {
            return Result<RequestIdentity>.Failure(ApplicationErrors.Auth.MissingSubjectId);
        }

        if (string.IsNullOrWhiteSpace(requestContext.TenantId))
        {
            return Result<RequestIdentity>.Failure(ApplicationErrors.Auth.MissingTenantId);
        }

        return Result<RequestIdentity>.Success(new RequestIdentity(requestContext.SubjectId, requestContext.TenantId));
    }
}
