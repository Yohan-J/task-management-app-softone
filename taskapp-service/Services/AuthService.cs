using backend.DTOs;
using backend.Repositories;

namespace backend.Services;

public class AuthService(IUserRepository userRepository) : IAuthService
{
    public async Task<AuthenticatedUser?> ValidateCredentialsAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByUsernameAndPasswordAsync(
            request.Username,
            request.Password,
            cancellationToken);

        return user is null ? null : new AuthenticatedUser(user.Id, user.Username);
    }
}
