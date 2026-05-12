using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public Task<AppUser?> GetByUsernameAndPasswordAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(
            u => u.Username == username && u.Password == password,
            cancellationToken);
}
