using HrCrm.Application.Interfaces;
using HrCrm.Domain.Entities;
using HrCrm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HrCrm.Infrastructure.Repositories;

public class DepartmentRepository(AppDbContext context) : IDepartmentRepository
{
    public Task<List<Department>> GetAllAsync() =>
        context.Departments.ToListAsync();

    public Task<Department?> GetByIdAsync(int id) =>
        context.Departments.FindAsync(id).AsTask();

    public async Task AddAsync(Department department)
    {
        await context.Departments.AddAsync(department);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Department department)
    {
        context.Departments.Update(department);
        await context.SaveChangesAsync();
    }

    public async Task RemoveAsync(Department department)
    {
        context.Departments.Remove(department);
        await context.SaveChangesAsync();
    }

    public Task<bool> HasEmployeesAsync(int id) =>
        context.Employees.AnyAsync(e => e.DepartmentId == id);

    public Task<bool> ExistsByNameAsync(string name, int? excludeId = null) =>
        context.Departments.AnyAsync(x =>
            x.Name == name && (excludeId == null || x.Id != excludeId));

    public async Task<List<Department>> GetPagedAsync(
        int skip, int take, string? search = null, string? sortBy = null, bool descending = false)
    {
        IQueryable<Department> query = context.Departments.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.Name.Contains(search));

        query = (sortBy?.ToLowerInvariant(), descending) switch
        {
            ("name", false) => query.OrderBy(d => d.Name).ThenBy(d => d.Id),
            ("name", true) => query.OrderByDescending(d => d.Name).ThenByDescending(d => d.Id),
            ("description", false) => query.OrderBy(d => d.Description).ThenBy(d => d.Id),
            ("description", true) => query.OrderByDescending(d => d.Description).ThenByDescending(d => d.Id),
            (_, true) => query.OrderByDescending(d => d.Id),
            _ => query.OrderBy(d => d.Id)
        };

        return await query.Skip(skip).Take(take).ToListAsync();
    }

    public Task<int> CountAsync(string? search = null)
    {
        IQueryable<Department> query = context.Departments;
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.Name.Contains(search));

        return query.CountAsync();
    }
}
