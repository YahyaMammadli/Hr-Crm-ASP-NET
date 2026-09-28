using HrCrm.Application.Common;
using HrCrm.Application.Interfaces;
using HrCrm.Application.Services;
using HrCrm.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HrCrm.ApplicationTest.Services.DepartmentTests;

public class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> repository = new();
    private readonly IDepartmentService service;

    public DepartmentServiceTests()
    {
        service = new DepartmentService(
            repository.Object,
            Mock.Of<ILogger<DepartmentService>>());
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsAlreadyTaken_ShouldReturnConflictAndNotSave()
    {
        repository
            .Setup(x => x.ExistsByNameAsync("Finance", null))
            .ReturnsAsync(true);

        var result = await service.CreateAsync("Finance", "Accounting");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        repository.Verify(x => x.AddAsync(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsFree_ShouldCreateAndSaveDepartment()
    {
        repository
            .Setup(x => x.ExistsByNameAsync("Finance", null))
            .ReturnsAsync(false);

        var result = await service.CreateAsync("Finance", "Accounting");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Finance", result.Value!.Name);
        Assert.Equal("Accounting", result.Value.Description);

        repository.Verify(
            x => x.AddAsync(It.Is<Department>(d =>
                d.Name == "Finance" &&
                d.Description == "Accounting")),
            Times.Once);
    }

    [Theory]
    [InlineData(1, 10, 1, 10, 0)]
    [InlineData(2, 10, 2, 10, 10)]
    [InlineData(0, 10, 1, 10, 0)]
    [InlineData(-3, 10, 1, 10, 0)]
    [InlineData(1, 0, 1, 10, 0)]
    [InlineData(1, -5, 1, 10, 0)]
    [InlineData(1, 51, 1, 50, 0)]
    [InlineData(3, 100, 3, 50, 100)]
    public async Task GetPagedAsync_ShouldNormalizePageAndPageSize(
        int requestedPage,
        int requestedPageSize,
        int expectedPage,
        int expectedPageSize,
        int expectedSkip)
    {
        repository
            .Setup(x => x.GetPagedAsync(expectedSkip, expectedPageSize, null, "id", false))
            .ReturnsAsync([]);
        repository
            .Setup(x => x.CountAsync(null))
            .ReturnsAsync(0);

        var result = await service.GetPagedAsync(requestedPage, requestedPageSize);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedPage, result.Value!.Page);
        Assert.Equal(expectedPageSize, result.Value.PageSize);
        repository.Verify(
            x => x.GetPagedAsync(expectedSkip, expectedPageSize, null, "id", false),
            Times.Once);
    }

    [Theory]
    [InlineData("name", false, "name", false)]
    [InlineData("description", true, "description", true)]
    [InlineData("ID", false, "id", false)]
    [InlineData("unknown", true, "id", true)]
    [InlineData("", false, "id", false)]
    [InlineData(null, true, "id", true)]
    public async Task GetPagedAsync_ShouldUseSafeSortFieldAndDirection(
        string? requestedSort,
        bool descending,
        string expectedSort,
        bool expectedDescending)
    {
        repository
            .Setup(x => x.GetPagedAsync(0, 10, null, expectedSort, expectedDescending))
            .ReturnsAsync([]);
        repository
            .Setup(x => x.CountAsync(null))
            .ReturnsAsync(0);

        var result = await service.GetPagedAsync(1, 10, null, requestedSort, descending);

        Assert.True(result.IsSuccess);
        repository.Verify(
            x => x.GetPagedAsync(0, 10, null, expectedSort, expectedDescending),
            Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_WhenSearchIsProvided_ShouldPassSearchToRepositoryAndCount()
    {
        repository
            .Setup(x => x.GetPagedAsync(0, 10, "Fin", "id", false))
            .ReturnsAsync([new Department { Id = 1, Name = "Finance" }]);
        repository
            .Setup(x => x.CountAsync("Fin"))
            .ReturnsAsync(1);

        var result = await service.GetPagedAsync(1, 10, "Fin");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(1, result.Value.TotalCount);
        repository.Verify(x => x.GetPagedAsync(0, 10, "Fin", "id", false), Times.Once);
        repository.Verify(x => x.CountAsync("Fin"), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenDepartmentDoesNotExist_ShouldReturnNotFoundAndNotUpdate()
    {
        repository.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Department?)null);

        var result = await service.UpdateAsync(99, "Finance", "Accounting");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        repository.Verify(x => x.UpdateAsync(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenDepartmentHasEmployees_ShouldReturnConflictAndNotRemove()
    {
        repository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new Department { Id = 1, Name = "Finance" });
        repository.Setup(x => x.HasEmployeesAsync(1)).ReturnsAsync(true);

        var result = await service.DeleteAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        repository.Verify(x => x.RemoveAsync(It.IsAny<Department>()), Times.Never);
    }
}
