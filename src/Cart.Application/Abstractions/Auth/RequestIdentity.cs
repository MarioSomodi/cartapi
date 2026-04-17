namespace Cart.Application.Abstractions.Auth;

public sealed record RequestIdentity(string SubjectId, string TenantId);
