using HrCrm.Application.Common;
using HrCrm.Application.Services;
using HrCrm.Domain.Entities;
using HrCrm.WebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrCrm.WebApi.Controllers;

[Route("api/[controller]")]
[Authorize]
public class EmployeesController(
    IEmployeeService employeeService,
    ILogger<EmployeesController> logger) : ApiBaseController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<EmployeeDto>>> GetAll([FromQuery] PaginationDto pagination)
    {
        var result = await employeeService.GetPagedAsync(
            pagination.Page, pagination.PageSize, pagination.Search, pagination.SortBy, pagination.Descending);

        if (!result.IsSuccess)
            return ToErrorResponse(result);

        var paged = result.Value!;
        return Ok(new PagedResult<EmployeeDto>
        {
            Items = paged.Items.Select(ToDto).ToList(),
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var result = await employeeService.GetAsync(id);
        if (!result.IsSuccess)
            return ToErrorResponse(result);

        return Ok(ToDto(result.Value!));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Create(CreateEmployeeDto request)
    {
        var result = await employeeService.CreateAsync(
            request.FullName, request.Email, request.Position, request.DepartmentId);

        if (!result.IsSuccess)
            return ToErrorResponse(result);

        var dto = ToDto(result.Value!);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Update(int id, UpdateEmployeeDto request)
    {
        var result = await employeeService.UpdateAsync(
            id, request.FullName, request.Email, request.Position, request.DepartmentId);

        if (!result.IsSuccess)
            return ToErrorResponse(result);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await employeeService.DeleteAsync(id);
        if (!result.IsSuccess)
            return ToErrorResponse(result);

        return NoContent();
    }

    private static EmployeeDto ToDto(Employee employee) =>
        new(employee.Id, employee.FullName, employee.Email, employee.Position, employee.DepartmentId);
}
