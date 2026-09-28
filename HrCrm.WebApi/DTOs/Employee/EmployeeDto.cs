namespace HrCrm.WebApi.DTOs;

public class EmployeeDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int DepartmentId { get; set; }

    public EmployeeDto() { }

    public EmployeeDto(int id, string fullName, string email, string position, int departmentId)
    {
        Id = id;
        FullName = fullName;
        Email = email;
        Position = position;
        DepartmentId = departmentId;
    }
}