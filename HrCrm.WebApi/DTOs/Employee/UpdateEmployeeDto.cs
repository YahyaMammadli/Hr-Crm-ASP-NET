using System.ComponentModel.DataAnnotations;

namespace HrCrm.WebApi.DTOs;

public class UpdateEmployeeDto
{
    [Required(ErrorMessage = "FullName can't be null")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email can't be null")]
    [EmailAddress(ErrorMessage = "Email format is invalid")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Position can't be null")]
    public string Position { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "DepartmentId must be positive")]
    public int DepartmentId { get; set; }
}