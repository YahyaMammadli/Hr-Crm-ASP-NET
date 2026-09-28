using HrCrm.Application.Interfaces;
using HrCrm.Infrastructure.Data;
using HrCrm.Infrastructure.Repositories;
using HrCrm.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrCrm.Infrastructure.Extenstion;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfigurationManager configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordManager, PasswordManager>();
        services.AddSingleton<ITokenGenerator, TokenGenerator>();

        return services;
    }
}
