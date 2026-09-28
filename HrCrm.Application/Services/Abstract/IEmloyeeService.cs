using HrCrm.Application.Common;
using HrCrm.Domain.Entities;

namespace HrCrm.Application.Services;

public interface IEmployeeService
{
    Task<Result<PagedResult<Employee>>> GetPagedAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool descending = false);
    Task<Result<List<Employee>>> GetAllAsync();
    Task<Result<Employee?>> GetAsync(int id);
    Task<Result<Employee?>> CreateAsync(string fullName, string email, string position, int departmentId);
    Task<Result> UpdateAsync(int id, string fullName, string email, string position, int departmentId);
    Task<Result> DeleteAsync(int id);
}
