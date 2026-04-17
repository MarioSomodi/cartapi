namespace Cart.Application.Abstractions.Auth;

public interface IRequestContext
{
    bool IsAuthenticated { get; }

    string? SubjectId { get; }

    string? TenantId { get; }
}
