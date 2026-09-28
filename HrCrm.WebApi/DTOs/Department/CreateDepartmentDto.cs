using System.ComponentModel.DataAnnotations;

namespace HrCrm.WebApi.DTOs;

public class CreateDepartmentDto
{
    [Required(ErrorMessage = "Name can't be null")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description can't be null")]
    [MaxLength(100, ErrorMessage = "Description length can not be more than 100 symb.")]
    public string Description { get; set; } = string.Empty;
}