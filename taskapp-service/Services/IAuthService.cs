using backend.DTOs;

namespace backend.Services;

public interface IAuthService
{
    Task<AuthenticatedUser?> ValidateCredentialsAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}
