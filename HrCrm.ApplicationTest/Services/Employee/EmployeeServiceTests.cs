using HrCrm.Application.Common;
using HrCrm.Application.Interfaces;
using HrCrm.Application.Services;
using HrCrm.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HrCrm.ApplicationTest.Services.EmployeeTests;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> employeeRepository = new();
    private readonly Mock<IDepartmentRepository> departmentRepository = new();
    private readonly IEmployeeService service;

    public EmployeeServiceTests()
    {
        service = new EmployeeService(
            employeeRepository.Object,
            departmentRepository.Object,
            Mock.Of<ILogger<EmployeeService>>());
    }

    [Fact]
    public async Task CreateAsync_WhenDepartmentDoesNotExist_ShouldReturnValidationAndNotSave()
    {
        departmentRepository.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Department?)null);

        var result = await service.CreateAsync(
            "John Smith", "john@example.com", "Developer", 99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        employeeRepository.Verify(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
        employeeRepository.Verify(x => x.AddAsync(It.IsAny<Employee>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailIsAlreadyTaken_ShouldReturnConflictAndNotSave()
    {
        departmentRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new Department { Id = 1, Name = "Finance" });
        employeeRepository.Setup(x => x.EmailExistsAsync("john@example.com", null)).ReturnsAsync(true);

        var result = await service.CreateAsync(
            "John Smith", "john@example.com", "Developer", 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        employeeRepository.Verify(x => x.AddAsync(It.IsAny<Employee>()), Times.Never);
    }

    [Theory]
    [InlineData("john@example.com", "john@example.com")]
    [InlineData("  john@example.com  ", "john@example.com")]
    [InlineData("JANE@example.com", "JANE@example.com")]
    public async Task CreateAsync_WhenDepartmentExistsAndEmailIsFree_ShouldCreateEmployee(
        string inputEmail,
        string expectedEmail)
    {
        departmentRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new Department { Id = 1, Name = "Finance" });
        employeeRepository.Setup(x => x.EmailExistsAsync(expectedEmail, null)).ReturnsAsync(false);

        var result = await service.CreateAsync(
            "John Smith", inputEmail, "Developer", 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedEmail, result.Value!.Email);
        Assert.Equal(1, result.Value.DepartmentId);
        employeeRepository.Verify(
            x => x.AddAsync(It.Is<Employee>(e =>
                e.FullName == "John Smith" &&
                e.Email == expectedEmail &&
                e.Position == "Developer" &&
                e.DepartmentId == 1)),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeDoesNotExist_ShouldReturnNotFoundAndNotUpdate()
    {
        employeeRepository.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Employee?)null);

        var result = await service.UpdateAsync(
            99, "John Smith", "john@example.com", "Developer", 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        employeeRepository.Verify(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
        employeeRepository.Verify(x => x.UpdateAsync(It.IsAny<Employee>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailIsTakenByAnotherEmployee_ShouldReturnConflictAndNotUpdate()
    {
        employeeRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new Employee
            {
                Id = 1,
                FullName = "John Smith",
                Email = "john@example.com",
                Position = "Developer",
                DepartmentId = 1
            });
        employeeRepository.Setup(x => x.EmailExistsAsync("jane@example.com", 1)).ReturnsAsync(true);

        var result = await service.UpdateAsync(
            1, "John Smith", "jane@example.com", "Senior Developer", 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        employeeRepository.Verify(x => x.UpdateAsync(It.IsAny<Employee>()), Times.Never);
        departmentRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenDepartmentIsChangedToMissingDepartment_ShouldReturnValidationAndNotUpdate()
    {
        employeeRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new Employee
            {
                Id = 1,
                FullName = "John Smith",
                Email = "john@example.com",
                Position = "Developer",
                DepartmentId = 1
            });
        employeeRepository.Setup(x => x.EmailExistsAsync("john@example.com", 1)).ReturnsAsync(false);
        departmentRepository.Setup(x => x.GetByIdAsync(2)).ReturnsAsync((Department?)null);

        var result = await service.UpdateAsync(
            1, "John Smith", "john@example.com", "Developer", 2);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        employeeRepository.Verify(x => x.UpdateAsync(It.IsAny<Employee>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenEmployeeDoesNotExist_ShouldReturnNotFoundAndNotRemove()
    {
        employeeRepository.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Employee?)null);

        var result = await service.DeleteAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        employeeRepository.Verify(x => x.RemoveAsync(It.IsAny<Employee>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenEmployeeExists_ShouldReturnSuccessAndRemoveEmployee()
    {
        var employee = new Employee
        {
            Id = 1,
            FullName = "John Smith",
            Email = "john@example.com",
            Position = "Developer",
            DepartmentId = 1
        };
        employeeRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(employee);

        var result = await service.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        employeeRepository.Verify(x => x.RemoveAsync(employee), Times.Once);
    }

    [Theory]
    [InlineData(1, 10, 1, 10, 0)]
    [InlineData(2, 10, 2, 10, 10)]
    [InlineData(0, 10, 1, 10, 0)]
    [InlineData(-2, 10, 1, 10, 0)]
    [InlineData(1, 0, 1, 10, 0)]
    [InlineData(1, 75, 1, 50, 0)]
    [InlineData(3, 25, 3, 25, 50)]
    public async Task GetPagedAsync_ShouldNormalizePagination(
        int requestedPage,
        int requestedPageSize,
        int expectedPage,
        int expectedPageSize,
        int expectedSkip)
    {
        employeeRepository
            .Setup(x => x.GetPagedAsync(expectedSkip, expectedPageSize, null, "id", false))
            .ReturnsAsync([]);
        employeeRepository.Setup(x => x.CountAsync(null)).ReturnsAsync(0);

        var result = await service.GetPagedAsync(requestedPage, requestedPageSize);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedPage, result.Value!.Page);
        Assert.Equal(expectedPageSize, result.Value.PageSize);
        employeeRepository.Verify(
            x => x.GetPagedAsync(expectedSkip, expectedPageSize, null, "id", false),
            Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_WhenSearchIsProvided_ShouldPassSearchToRepositoryAndCount()
    {
        employeeRepository
            .Setup(x => x.GetPagedAsync(10, 10, "john", "id", false))
            .ReturnsAsync([new Employee { Id = 2, FullName = "John Smith" }]);
        employeeRepository.Setup(x => x.CountAsync("john")).ReturnsAsync(1);

        var result = await service.GetPagedAsync(2, 10, "john");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(1, result.Value.TotalCount);
        employeeRepository.Verify(x => x.GetPagedAsync(10, 10, "john", "id", false), Times.Once);
        employeeRepository.Verify(x => x.CountAsync("john"), Times.Once);
    }
}
