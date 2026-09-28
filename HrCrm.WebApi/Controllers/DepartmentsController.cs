using HrCrm.Application.Common;
using HrCrm.Application.Services;
using HrCrm.Domain.Entities;
using HrCrm.WebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrCrm.WebApi.Controllers;

[Route("api/[controller]")]
[Authorize]
public class DepartmentsController(
    IDepartmentService departmentService,
    ILogger<DepartmentsController> logger) : ApiBaseController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<DepartmentDto>>> GetAll([FromQuery] PaginationDto pagination)
    {
        var result = await departmentService.GetPagedAsync(
            pagination.Page, pagination.PageSize, pagination.Search, pagination.SortBy, pagination.Descending);

        if (!result.IsSuccess)
            return ToErrorResponse(result);

        var paged = result.Value!;
        return Ok(new PagedResult<DepartmentDto>
        {
            Items = paged.Items.Select(ToDto).ToList(),
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DepartmentDto>> GetById(int id)
    {
        var result = await departmentService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return ToErrorResponse(result);

        return Ok(ToDto(result.Value!));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Create(CreateDepartmentDto request)
    {
        var result = await departmentService.CreateAsync(request.Name, request.Description);
        if (!result.IsSuccess)
            return ToErrorResponse(result);

        var dto = ToDto(result.Value!);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Update(int id, UpdateDepartmentDto request)
    {
        var result = await departmentService.UpdateAsync(id, request.Name, request.Description);
        if (!result.IsSuccess)
            return ToErrorResponse(result);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await departmentService.DeleteAsync(id);
        if (!result.IsSuccess)
            return ToErrorResponse(result);

        return NoContent();
    }

    private static DepartmentDto ToDto(Department department) =>
        new(department.Id, department.Name, department.Description);
}
