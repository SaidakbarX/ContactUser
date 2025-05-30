using Xunit;
using Moq;
using ContactUser.Application.Services;
using ContactUser.Application.Interfaces;
using ContactUser.Domain.Entities;
using ContactUser.Application.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RoleServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepoMock = new();

    private RoleService CreateService() => new(_roleRepoMock.Object);

    [Fact]
    public async Task GetAllRolesAsync_ReturnsListOfRoles()
    {
        var roles = new List<UserRole>
        {
            new UserRole { Id = 1, Name = "Admin", Description = "Administrator" },
            new UserRole { Id = 2, Name = "User", Description = "Regular User" }
        };

        _roleRepoMock.Setup(r => r.GetAllRolesAsync()).ReturnsAsync(roles);

        var service = CreateService();

        var result = await service.GetAllRolesAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Name == "Admin");
        Assert.Contains(result, r => r.Name == "User");
    }

    [Fact]
    public async Task GetRoleIdAsync_ExistingRole_ReturnsId()
    {
        _roleRepoMock.Setup(r => r.GetRoleIdAsync("Admin")).ReturnsAsync(1);

        var service = CreateService();

        var result = await service.GetRoleIdAsync("Admin");

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetAllUsersByRoleAsync_ReturnsUsers()
    {
        var users = new List<User>
        {
            new User
            {
                UserId = 1,
                UserName = "ali",
                FirstName = "Ali",
                LastName = "Valiyev",
                Email = "ali@mail.com",
                PhoneNumber = "998901112233",
                Role = new UserRole { Name = "Admin" }
            },
            new User
            {
                UserId = 2,
                UserName = "vali",
                FirstName = "Vali",
                LastName = "Aliyev",
                Email = "vali@mail.com",
                PhoneNumber = "998901122233",
                Role = new UserRole { Name = "Admin" }
            }
        };

        _roleRepoMock.Setup(r => r.GetAllUsersByRoleAsync("Admin")).ReturnsAsync(users);

        var service = CreateService();

        var result = await service.GetAllUsersByRoleAsync("Admin");

        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.UserName == "ali");
        Assert.Contains(result, u => u.UserName == "vali");
    }
}
