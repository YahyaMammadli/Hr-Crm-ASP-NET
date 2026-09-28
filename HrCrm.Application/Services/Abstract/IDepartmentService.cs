using HrCrm.Application.Common;
using HrCrm.Domain.Entities;

namespace HrCrm.Application.Services;

public interface IDepartmentService
{
    Task<Result<PagedResult<Department>>> GetPagedAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool descending = false);
    Task<Result<List<Department>>> GetAllAsync();
    Task<Result<Department?>> GetByIdAsync(int id);
    Task<Result<Department>> CreateAsync(string name, string description);
    Task<Result> UpdateAsync(int id, string name, string description);
    Task<Result> DeleteAsync(int id);
}
