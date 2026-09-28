using HrCrm.Domain.Entities;

namespace HrCrm.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> ExistsByUsernameAsync(string username);
    Task AddAsync(User user);
}
