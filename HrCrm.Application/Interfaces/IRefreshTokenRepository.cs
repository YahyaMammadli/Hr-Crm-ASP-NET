using HrCrm.Domain.Entities;

namespace HrCrm.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    Task RemoveAsync(RefreshToken refreshToken);
    Task RemoveExpiredForUserAsync(int userId);
}
