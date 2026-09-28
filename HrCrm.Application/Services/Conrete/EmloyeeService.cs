using HrCrm.Application.Common;
using HrCrm.Application.Interfaces;
using HrCrm.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace HrCrm.Application.Services;

public class EmployeeService(
    IEmployeeRepository employeeRepository,
    IDepartmentRepository departmentRepository,
    ILogger<EmployeeService> logger) : IEmployeeService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    public async Task<Result<List<Employee>>> GetAllAsync()
    {
        var employees = await employeeRepository.GetAllAsync();
        return Result<List<Employee>>.Success(employees);
    }

    public async Task<Result<Employee?>> GetAsync(int id)
    {
        var emp = await employeeRepository.GetByIdAsync(id);
        if (emp is null)
            return Result<Employee?>.Failure($"Employee with id {id} does not exist", ErrorType.NotFound);

        return Result<Employee?>.Success(emp);
    }

    public async Task<Result<Employee?>> CreateAsync(
        string fullName, string email, string position, int departmentId)
    {
        var department = await departmentRepository.GetByIdAsync(departmentId);
        if (department is null)
            return Result<Employee?>.Failure(
                $"Department with id {departmentId} does not exist", ErrorType.Validation);

        var normalizedEmail = email.Trim();

        if (await employeeRepository.EmailExistsAsync(normalizedEmail))
            return Result<Employee?>.Failure(
                "Can not insert duplicate employees by email", ErrorType.Conflict);

        var employee = new Employee
        {
            FullName = fullName,
            Email = normalizedEmail,
            Position = position,
            DepartmentId = departmentId
        };

        await employeeRepository.AddAsync(employee);
        return Result<Employee?>.Success(employee);
    }

    public async Task<Result> UpdateAsync(
        int id, string fullName, string email, string position, int departmentId)
    {
        var employee = await employeeRepository.GetByIdAsync(id);
        if (employee is null)
            return Result.Failure($"Employee with id {id} does not exist", ErrorType.NotFound);

        var normalizedEmail = email.Trim();

        if (await employeeRepository.EmailExistsAsync(normalizedEmail, id))
            return Result.Failure(
                "Can not insert duplicate employees by email", ErrorType.Conflict);

        if (employee.DepartmentId != departmentId)
        {
            var department = await departmentRepository.GetByIdAsync(departmentId);
            if (department is null)
                return Result.Failure(
                    $"Department with id {departmentId} does not exist", ErrorType.Validation);
        }

        employee.FullName = fullName;
        employee.Email = normalizedEmail;
        employee.Position = position;
        employee.DepartmentId = departmentId;

        await employeeRepository.UpdateAsync(employee);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var employee = await employeeRepository.GetByIdAsync(id);
        if (employee is null)
            return Result.Failure($"Employee with id {id} does not exist", ErrorType.NotFound);

        await employeeRepository.RemoveAsync(employee);
        return Result.Success();
    }

    public async Task<Result<PagedResult<Employee>>> GetPagedAsync(
        int page, int pageSize, string? search = null, string? sortBy = null, bool descending = false)
    {
        page = Math.Max(1, page);
        pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        sortBy = NormalizeEmployeeSort(sortBy);
        var skip = (page - 1) * pageSize;

        var items = await employeeRepository.GetPagedAsync(skip, pageSize, search, sortBy, descending);
        var totalCount = await employeeRepository.CountAsync(search);

        return Result<PagedResult<Employee>>.Success(new PagedResult<Employee>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    private static string NormalizeEmployeeSort(string? sortBy) =>
        sortBy?.Trim().ToLowerInvariant() switch
        {
            "fullname" => "fullname",
            "email" => "email",
            "position" => "position",
            "departmentid" => "departmentid",
            "id" or null or "" => "id",
            _ => "id"
        };
}
