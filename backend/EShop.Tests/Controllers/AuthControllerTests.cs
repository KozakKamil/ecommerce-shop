using EShop.API.Controllers;
using EShop.API.DTOs;
using EShop.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EShop.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly IConfiguration _config;

    public AuthControllerTests()
    {
        var store = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null!, null!, null!, null!);

        _config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "Jwt:Key", "SuperSecretTestKeyThatIsLongEnough123!" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" }
        }).Build();
    }
    
    [Fact]
    public async Task Login_InvalidEmail_ReturnsUnauthorized()
    {
        _userManagerMock
            .Setup(x => x.FindByEmailAsync("notexist@test.com"))
            .ReturnsAsync((AppUser?)null);

        var controller = new AuthController(_userManagerMock.Object, _roleManagerMock.Object, _config);
        var result = await controller.Login(new LoginDto { Email = "notexist@test.com", Password = "Test123!" });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var user = new AppUser { Email = "test@test.com", FirstName = "Jan", LastName = "Kowalski" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@test.com")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "WrongPass")).ReturnsAsync(false);

        var controller = new AuthController(_userManagerMock.Object, _roleManagerMock.Object, _config);
        var result = await controller.Login(new LoginDto { Email = "test@test.com", Password = "WrongPass" });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        var user = new AppUser { Id = "u1", Email = "test@test.com", UserName = "test@test.com", FirstName = "Jan", LastName = "Kowalski" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@test.com")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "Test123!")).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

        var controller = new AuthController(_userManagerMock.Object, _roleManagerMock.Object, _config);
        var result = await controller.Login(new LoginDto { Email = "test@test.com", Password = "Test123!" });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.NotEmpty(response.Token);
        Assert.Equal("test@test.com", response.Email);
    }

    [Fact]
    public async Task Register_InvalidData_ReturnsBadRequest()
    {
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email zajęty" }));

        var controller = new AuthController(_userManagerMock.Object, _roleManagerMock.Object, _config);
        var result = await controller.Register(new RegisterDto
        {
            Email = "exists@test.com",
            Password = "Test123!",
            FirstName = "Jan",
            LastName = "Kowalski"
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Register_ValidData_ReturnsOkWithToken()
    {
        var user = new AppUser { Id = "u1", Email = "new@test.com", UserName = "new@test.com", FirstName = "Anna", LastName = "Nowak" };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), "Test123!"))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock.Setup(x => x.RoleExistsAsync("User")).ReturnsAsync(true);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync("new@test.com"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<AppUser>())).ReturnsAsync(new List<string> { "User" });

        var controller = new AuthController(_userManagerMock.Object, _roleManagerMock.Object, _config);
        var result = await controller.Register(new RegisterDto
        {
            Email = "new@test.com",
            Password = "Test123!",
            FirstName = "Anna",
            LastName = "Nowak"
        });

        Assert.IsType<OkObjectResult>(result.Result);
    }
}