using Xunit;
using Moq;
using ContactUser.Application.Services;
using ContactUser.Application.Interfaces;
using ContactUser.Domain.Entities;
using ContactUser.Core.Errors;
using System.Threading.Tasks;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();

    private UserService CreateService() => new(_userRepoMock.Object);

    [Fact]
    public async Task DeleteUserByIdAsync_SuperAdmin_CanDeleteAnyone()
    {
        _userRepoMock.Setup(r => r.DeleteUserByIdAsync(1)).Returns(Task.CompletedTask).Verifiable();

        var service = CreateService();

        await service.DeleteUserByIdAsync(1, "SuperAdmin");

        _userRepoMock.Verify(r => r.DeleteUserByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteUserByIdAsync_Admin_CanDeleteUser()
    {
        var user = new User
        {
            UserId = 2,
            Role = new UserRole { Name = "User" }
        };

        _userRepoMock.Setup(r => r.SelectUserByIdAync(2)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.DeleteUserByIdAsync(2)).Returns(Task.CompletedTask).Verifiable();

        var service = CreateService();

        await service.DeleteUserByIdAsync(2, "Admin");

        _userRepoMock.Verify(r => r.DeleteUserByIdAsync(2), Times.Once);
    }

    [Fact]
    public async Task DeleteUserByIdAsync_Admin_CannotDeleteAdminOrSuperAdmin()
    {
        var user = new User
        {
            UserId = 3,
            Role = new UserRole { Name = "Admin" }
        };

        _userRepoMock.Setup(r => r.SelectUserByIdAync(3)).ReturnsAsync(user);

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<NotAllowedException>(() => service.DeleteUserByIdAsync(3, "Admin"));

        Assert.Equal("Admin can not delete Admin or SuperAdmin", ex.Message);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_CallsRepository()
    {
        _userRepoMock.Setup(r => r.UpdateUserRoleAsync(1, "Admin")).Returns(Task.CompletedTask).Verifiable();

        var service = CreateService();

        await service.UpdateUserRoleAsync(1, "Admin");

        _userRepoMock.Verify(r => r.UpdateUserRoleAsync(1, "Admin"), Times.Once);
    }
}
