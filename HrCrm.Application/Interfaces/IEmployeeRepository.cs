using HrCrm.Domain.Entities;

namespace HrCrm.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<List<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task RemoveAsync(Employee employee);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    Task<List<Employee>> GetPagedAsync(int skip, int take, string? search = null, string? sortBy = null, bool descending = false);
    Task<int> CountAsync(string? search = null);
}
