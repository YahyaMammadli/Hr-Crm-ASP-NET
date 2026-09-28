using HrCrm.Application.Common;
using HrCrm.Application.Interfaces;
using HrCrm.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace HrCrm.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<AuthService> logger,
    IPasswordManager passwordManager,
    ITokenGenerator tokenGenerator) : IAuthService
{
    public async Task<Result> RegisterAsync(string username, string password)
    {
        if (await userRepository.ExistsByUsernameAsync(username))
            return Result.Failure($"Username '{username}' is already taken", ErrorType.Conflict);

        var user = new User
        {
            Username = username,
            PasswordHash = passwordManager.Hash(password),
            Role = UserRole.User
        };

        await userRepository.AddAsync(user);
        return Result.Success();
    }

    public async Task<Result<AuthResult>> LoginAsync(string username, string password)
    {
        var user = await userRepository.GetByUsernameAsync(username);
        if (user is null || !passwordManager.Verify(password, user.PasswordHash))
            return Result<AuthResult>.Failure("Invalid username or password", ErrorType.Unauthorized);

        await refreshTokenRepository.RemoveExpiredForUserAsync(user.Id);

        return await IssueTokensAsync(user);
    }

    public async Task<Result<AuthResult>> RefreshAsync(string refreshToken)
    {
        var stored = await refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (stored is null)
            return Result<AuthResult>.Failure("Invalid refresh token", ErrorType.Unauthorized);

        if (stored.ExpiresAt <= DateTime.UtcNow)
        {
            await refreshTokenRepository.RemoveAsync(stored);
            return Result<AuthResult>.Failure("Refresh token has expired", ErrorType.Unauthorized);
        }

        var user = stored.User;
        await refreshTokenRepository.RemoveAsync(stored);
        await refreshTokenRepository.RemoveExpiredForUserAsync(user.Id);

        return await IssueTokensAsync(user);
    }

    private async Task<Result<AuthResult>> IssueTokensAsync(User user)
    {
        var accessToken = tokenGenerator.GenerateAccessToken(user);
        var refreshToken = tokenGenerator.GenerateRefreshToken();

        await refreshTokenRepository.AddAsync(new RefreshToken
        {
            Token = refreshToken.Token,
            UserId = user.Id,
            ExpiresAt = refreshToken.ExpiresAt
        });

        logger.LogInformation("Tokens issued for user {Id}", user.Id);

        return Result<AuthResult>.Success(new AuthResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        });
    }
}
