using HrCrm.Application.Interfaces;
using HrCrm.Domain.Entities;
using HrCrm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HrCrm.Infrastructure.Repositories;

public class RefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(string token) =>
        context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token);

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await context.RefreshTokens.AddAsync(refreshToken);
        await context.SaveChangesAsync();
    }

    public async Task RemoveAsync(RefreshToken refreshToken)
    {
        context.RefreshTokens.Remove(refreshToken);
        await context.SaveChangesAsync();
    }

    public async Task RemoveExpiredForUserAsync(int userId)
    {
        var expired = await context.RefreshTokens
            .Where(x => x.UserId == userId && x.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        if (expired.Count == 0)
            return;

        context.RefreshTokens.RemoveRange(expired);
        await context.SaveChangesAsync();
    }
}
