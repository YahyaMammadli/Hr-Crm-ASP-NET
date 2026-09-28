# HR CRM

HR CRM is a backend application for managing company departments and employees.

The project demonstrates a layered ASP.NET Core architecture, Entity Framework Core, repository/service patterns, JWT authentication, refresh tokens, pagination, filtering, sorting, and unit testing with xUnit and Moq.

---

## Features

### Departments

- Create departments
- Get department by ID
- Update department
- Delete department
- Prevent duplicate department names
- Prevent deletion of departments that contain employees
- Paginated department listing
- Search departments by name
- Sort departments by selected fields
- Sort in ascending or descending order
- Calculate total records and total pages

### Employees

- Create employees
- Get employee by ID
- Update employee
- Delete employee
- Prevent duplicate employee emails
- Validate that the selected department exists
- Paginated employee listing
- Search/filter employees
- Sorting support
- Protect employee data with authentication and authorization

### Authentication & Authorization

- User registration
- User login
- JWT access tokens
- Refresh tokens
- Refresh token rotation
- Expired refresh token cleanup
- Role-based authorization
- Current-user information endpoint
- Admin-only operations

### Testing

The project contains a dedicated unit test project:

- xUnit
- Moq
- `[Fact]`
- `[Theory]`
- `[InlineData]`
- Mocked repositories and dependencies
- Verification of service side effects
- Boundary-value testing
- Isolated unit tests without a real database

---

# Architecture

The solution follows a layered architecture:

```text
HrCrm
│
├── HrCrm.Domain
│
├── HrCrm.Application
│
├── HrCrm.Infrastructure
│
├── HrCrm.WebApi
│
├── HrCrm.ApplicationTest
│
└── HrCrm.sln
