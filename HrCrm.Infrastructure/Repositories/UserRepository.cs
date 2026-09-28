using HrCrm.Application.Interfaces;
using HrCrm.Domain.Entities;
using HrCrm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HrCrm.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username) =>
        context.Users.FirstOrDefaultAsync(x => x.Username == username);

    public Task<bool> ExistsByUsernameAsync(string username) =>
        context.Users.AnyAsync(x => x.Username == username);

    public async Task AddAsync(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }
}
