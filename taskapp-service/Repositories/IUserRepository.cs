using backend.Models;

namespace backend.Repositories;

public interface IUserRepository
{
    Task<AppUser?> GetByUsernameAndPasswordAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);
}
