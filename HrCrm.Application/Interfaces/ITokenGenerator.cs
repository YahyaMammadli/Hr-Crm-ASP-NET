using HrCrm.Application.Common;
using HrCrm.Domain.Entities;

namespace HrCrm.Application.Interfaces;

public interface ITokenGenerator
{
    string GenerateAccessToken(User user);
    RefreshTokenResult GenerateRefreshToken();
}
