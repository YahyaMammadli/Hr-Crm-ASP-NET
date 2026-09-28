using HrCrm.Domain.Entities;

namespace HrCrm.Application.Interfaces;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task AddAsync(Department department);
    Task UpdateAsync(Department department);
    Task RemoveAsync(Department department);
    Task<bool> HasEmployeesAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<List<Department>> GetPagedAsync(int skip, int take, string? search = null, string? sortBy = null, bool descending = false);
    Task<int> CountAsync(string? search = null);
}
