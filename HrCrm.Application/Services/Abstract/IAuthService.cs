using HrCrm.Application.Common;

namespace HrCrm.Application.Services;

public interface IAuthService
{
    Task<Result> RegisterAsync(string username, string password);
    Task<Result<AuthResult>> LoginAsync(string username, string password);
    Task<Result<AuthResult>> RefreshAsync(string refreshToken);
}
