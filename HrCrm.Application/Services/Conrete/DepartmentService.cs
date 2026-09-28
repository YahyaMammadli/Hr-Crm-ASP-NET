using HrCrm.Application.Common;
using HrCrm.Application.Interfaces;
using HrCrm.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace HrCrm.Application.Services;

public class DepartmentService(
    IDepartmentRepository departmentRepository,
    ILogger<DepartmentService> logger) : IDepartmentService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    public async Task<Result<List<Department>>> GetAllAsync()
    {
        var deps = await departmentRepository.GetAllAsync();
        return Result<List<Department>>.Success(deps);
    }

    public async Task<Result<Department?>> GetByIdAsync(int id)
    {
        var dep = await departmentRepository.GetByIdAsync(id);
        if (dep is null)
        {
            logger.LogWarning("Department with {Id} does not exist", id);
            return Result<Department?>.Failure($"Department with id {id} does not exist", ErrorType.NotFound);
        }

        return Result<Department?>.Success(dep);
    }

    public async Task<Result<Department>> CreateAsync(string name, string description)
    {
        if (await departmentRepository.ExistsByNameAsync(name))
        {
            logger.LogWarning("Cannot create department {Name}: duplicate name", name);
            return Result<Department>.Failure("Can not insert duplicate departments", ErrorType.Conflict);
        }

        var dep = new Department { Name = name, Description = description };
        await departmentRepository.AddAsync(dep);
        return Result<Department>.Success(dep);
    }

    public async Task<Result> UpdateAsync(int id, string name, string description)
    {
        var dep = await departmentRepository.GetByIdAsync(id);
        if (dep is null)
            return Result.Failure($"Department with id {id} does not exist", ErrorType.NotFound);

        if (await departmentRepository.ExistsByNameAsync(name, id))
            return Result.Failure("Can not insert duplicate departments", ErrorType.Conflict);

        dep.Name = name;
        dep.Description = description;
        await departmentRepository.UpdateAsync(dep);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var dep = await departmentRepository.GetByIdAsync(id);
        if (dep is null)
            return Result.Failure($"Department with id {id} does not exist", ErrorType.NotFound);

        if (await departmentRepository.HasEmployeesAsync(id))
            return Result.Failure($"Department with id {id} contains some employees", ErrorType.Conflict);

        await departmentRepository.RemoveAsync(dep);
        return Result.Success();
    }

    public async Task<Result<PagedResult<Department>>> GetPagedAsync(
        int page, int pageSize, string? search = null, string? sortBy = null, bool descending = false)
    {
        page = Math.Max(1, page);
        pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        sortBy = NormalizeDepartmentSort(sortBy);
        var skip = (page - 1) * pageSize;

        var items = await departmentRepository.GetPagedAsync(skip, pageSize, search, sortBy, descending);
        var totalCount = await departmentRepository.CountAsync(search);

        return Result<PagedResult<Department>>.Success(new PagedResult<Department>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    private static string NormalizeDepartmentSort(string? sortBy) =>
        sortBy?.Trim().ToLowerInvariant() switch
        {
            "name" => "name",
            "description" => "description",
            "id" or null or "" => "id",
            _ => "id"
        };
}
