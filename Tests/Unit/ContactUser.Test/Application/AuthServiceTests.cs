using Xunit;
using Moq;
using FluentValidation;
using ContactUser.Application.Services;
using ContactUser.Application.Interfaces;
using ContactUser.Application.Dtos;
using ContactUser.Application.Settings;
using ContactUser.Domain.Entities;
using ContactUser.Core.Errors;
using System.Security.Claims;
using System.Data;
using ContactUser.Application.Helpers;
using ContactUser.Application.Helpers.Security;

public class AuthServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepoMock = new();
    private readonly Mock<IValidator<UserCreateDto>> _validatorMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly JwtAppSettings _jwtSettings = new JwtAppSettings(
        issuer: "TestIssuer",
        audience: "TestAudience",
        securityKey: "SuperSecretTestKey1234567890",
        lifetime: "60");
    private readonly Mock<IValidator<UserCreateDto>> _loginValidatorMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();

    private AuthService CreateService() =>
        new(
            _roleRepoMock.Object,
            _validatorMock.Object,
            _userRepoMock.Object,
            _tokenServiceMock.Object,
            _jwtSettings,
            _loginValidatorMock.Object,
            _refreshTokenRepoMock.Object
        );

    [Fact]
    public async Task SignUpUserAsync_ValidData_ReturnsUserId()
    {
        var userDto = new UserCreateDto
        {
            FirstName = "Ali",
            LastName = "Valiyev",
            UserName = "aliv",
            Email = "ali@mail.com",
            PhoneNumber = "998901112233",
            Password = "secret"
        };

        _validatorMock.Setup(v => v.ValidateAsync(userDto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _roleRepoMock.Setup(r => r.GetRoleIdAsync("User")).ReturnsAsync(1);
        _userRepoMock.Setup(r => r.InsertUserAync(It.IsAny<User>())).ReturnsAsync(10);

        var service = CreateService();

        var result = await service.SignUpUserAsync(userDto);

        Assert.Equal(10, result);
    }

    [Fact]
    public async Task LoginUserAsync_ValidCredentials_ReturnsToken()
    {
        var userLoginDto = new UserLoginDto
        {
            UserName = "aliv",
            Password = "password"
        };

        var hashed = PasswordHasher.Hasher("password");

        var user = new User
        {
            UserId = 1,
            UserName = "aliv",
            FirstName = "Ali",
            LastName = "Valiyev",
            Email = "ali@mail.com",
            PhoneNumber = "998901112233",
            Password = hashed.Hash,   
            Salt = hashed.Salt,       
            Role = new UserRole { Name = "User" }
        };

        _userRepoMock.Setup(u => u.SelectUserByUserNameAync(userLoginDto.UserName))
            .ReturnsAsync(user);

        _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<UserGetDto>()))
            .Returns("token");

        _tokenServiceMock.Setup(t => t.GenerateRefreshToken())
            .Returns("refresh_token");

        _refreshTokenRepoMock.Setup(r => r.AddRefreshToken(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.LoginUserAsync(userLoginDto);

        Assert.Equal("token", result.AccessToken);
        Assert.Equal("refresh_token", result.RefreshToken);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(24, result.Expires);
    }


    [Fact]
    public async Task LoginUserAsync_InvalidPassword_ThrowsUnauthorized()
    {
        var userLoginDto = new UserLoginDto
        {
            UserName = "aliv",
            Password = "wrongpass"
        };

        var hashResult = PasswordHasher.Hasher("correctpass");

        var user = new User
        {
            UserId = 1,
            UserName = "aliv",
            Password = hashResult.Hash,
            Salt = hashResult.Salt,
            Role = new UserRole { Name = "User" }
        };

        _userRepoMock.Setup(u => u.SelectUserByUserNameAync(userLoginDto.UserName))
            .ReturnsAsync(user);

        var service = CreateService();

        await Assert.ThrowsAsync<UnauthorizedException>(() => service.LoginUserAsync(userLoginDto));
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewToken()
    {
        var userId = 1L;
        var request = new RefreshRequestDto
        {
            AccessToken = "expired_token",
            RefreshToken = "refresh_token"
        };

        var claims = new List<Claim>
        {
            new Claim("UserId", userId.ToString())
        };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _tokenServiceMock.Setup(t => t.GetPrincipalFromExpiredToken(request.AccessToken))
            .Returns(principal);

        _refreshTokenRepoMock.Setup(r => r.SelectRefreshToken("refresh_token", userId))
            .ReturnsAsync(new RefreshToken
            {
                Token = "refresh_token",
                Expires = DateTime.UtcNow.AddDays(1),
                IsRevoked = false,
                UserId = userId
            });

        var user = new User
        {
            UserId = userId,
            UserName = "aliv",
            FirstName = "Ali",
            LastName = "Valiyev",
            Email = "ali@mail.com",
            PhoneNumber = "998901112233",
            Role = new UserRole { Name = "User" }
        };

        _userRepoMock.Setup(u => u.SelectUserByIdAync(userId))
            .ReturnsAsync(user);

        _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<UserGetDto>()))
            .Returns("new_token");

        _tokenServiceMock.Setup(t => t.GenerateRefreshToken())
            .Returns("new_refresh");

        _refreshTokenRepoMock.Setup(r => r.AddRefreshToken(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.RefreshTokenAsync(request);

        Assert.Equal("new_token", result.AccessToken);
        Assert.Equal("new_refresh", result.RefreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidToken_ThrowsUnauthorized()
    {
        var userId = 1L;
        var request = new RefreshRequestDto
        {
            AccessToken = "expired_token",
            RefreshToken = "invalid_token"
        };

        var claims = new List<Claim>
        {
            new Claim("UserId", userId.ToString())
        };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _tokenServiceMock.Setup(t => t.GetPrincipalFromExpiredToken(request.AccessToken))
            .Returns(principal);

        _refreshTokenRepoMock.Setup(r => r.SelectRefreshToken("invalid_token", userId))
            .ReturnsAsync((RefreshToken?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<UnauthorizedException>(() => service.RefreshTokenAsync(request));
    }

    [Fact]
    public async Task LogOut_ValidToken_CallsRepo()
    {
        var token = "some_refresh_token";

        _refreshTokenRepoMock.Setup(r => r.DeleteRefreshToken(token))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var service = CreateService();

        await service.LogOut(token);

        _refreshTokenRepoMock.Verify(r => r.DeleteRefreshToken(token), Times.Once);
    }
}
