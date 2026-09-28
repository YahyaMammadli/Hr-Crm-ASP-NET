namespace HrCrm.WebApi.DTOs;

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DepartmentDto() { }

    public DepartmentDto(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
}