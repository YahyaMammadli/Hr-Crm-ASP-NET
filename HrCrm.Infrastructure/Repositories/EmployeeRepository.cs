using HrCrm.Application.Interfaces;
using HrCrm.Domain.Entities;
using HrCrm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HrCrm.Infrastructure.Repositories;

public class EmployeeRepository(AppDbContext context) : IEmployeeRepository
{
    public Task<List<Employee>> GetAllAsync() =>
        context.Employees.ToListAsync();

    public Task<Employee?> GetByIdAsync(int id) =>
        context.Employees.FindAsync(id).AsTask();

    public async Task AddAsync(Employee employee)
    {
        await context.Employees.AddAsync(employee);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Employee employee)
    {
        context.Employees.Update(employee);
        await context.SaveChangesAsync();
    }

    public async Task RemoveAsync(Employee employee)
    {
        context.Employees.Remove(employee);
        await context.SaveChangesAsync();
    }

    public Task<bool> EmailExistsAsync(string email, int? excludeId = null) =>
        context.Employees.AnyAsync(x =>
            x.Email == email && (excludeId == null || x.Id != excludeId));

    public async Task<List<Employee>> GetPagedAsync(
        int skip, int take, string? search = null, string? sortBy = null, bool descending = false)
    {
        IQueryable<Employee> query = context.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e =>
                e.FullName.Contains(search) ||
                e.Email.Contains(search) ||
                e.Position.Contains(search));

        query = (sortBy?.ToLowerInvariant(), descending) switch
        {
            ("fullname", false) => query.OrderBy(e => e.FullName).ThenBy(e => e.Id),
            ("fullname", true) => query.OrderByDescending(e => e.FullName).ThenByDescending(e => e.Id),
            ("email", false) => query.OrderBy(e => e.Email).ThenBy(e => e.Id),
            ("email", true) => query.OrderByDescending(e => e.Email).ThenByDescending(e => e.Id),
            ("position", false) => query.OrderBy(e => e.Position).ThenBy(e => e.Id),
            ("position", true) => query.OrderByDescending(e => e.Position).ThenByDescending(e => e.Id),
            ("departmentid", false) => query.OrderBy(e => e.DepartmentId).ThenBy(e => e.Id),
            ("departmentid", true) => query.OrderByDescending(e => e.DepartmentId).ThenByDescending(e => e.Id),
            (_, true) => query.OrderByDescending(e => e.Id),
            _ => query.OrderBy(e => e.Id)
        };

        return await query.Skip(skip).Take(take).ToListAsync();
    }

    public Task<int> CountAsync(string? search = null)
    {
        IQueryable<Employee> query = context.Employees;

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e =>
                e.FullName.Contains(search) ||
                e.Email.Contains(search) ||
                e.Position.Contains(search));

        return query.CountAsync();
    }
}
