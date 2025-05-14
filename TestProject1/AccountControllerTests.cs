using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using UserInfoManager.Controllers.Api;
using UserInfoManager.Data;
using UserInfoManager.Models;
using UserInfoManager.Service;

namespace TestProject1;

public class AccountControllerTests
{
    private readonly AccountController _controller;
    private readonly Mock<IUsersService> _mockUserService;
    private readonly Mock<IMembersDataAccess> _mockMembersDataAccess;
    private readonly Mock<IUserContactDataAccess> _mockUserContactDataAccess;
    private readonly Mock<IUserAddressDataAccess> _mockUserAddressDataAccess;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<AdoNetHelper> _mockAdoNetHelper;

    public AccountControllerTests()
    {
        _mockUserService = new Mock<IUsersService>();
        _mockAdoNetHelper = new Mock<AdoNetHelper>(MockBehavior.Default, "");
        _mockMembersDataAccess = new Mock<IMembersDataAccess>();
        _mockUserContactDataAccess = new Mock<IUserContactDataAccess>();
        _mockUserAddressDataAccess = new Mock<IUserAddressDataAccess>();
        _mockConfiguration = new Mock<IConfiguration>();

        _controller = new AccountController(
            _mockUserService.Object,
            _mockConfiguration.Object,
            _mockMembersDataAccess.Object,
            _mockUserContactDataAccess.Object,
            _mockUserAddressDataAccess.Object);
    }

    /// <summary>
    /// 修改用户信息
    /// </summary>
    [Fact]
    public void UpdateUser_ValidInput_ReturnsOk()
    {
        // Arrange
        var model = new MembersDto
        {
            Name = "Test Name",
            UserName = "testuser",
            Name_Visible = true,
            Name_Last = "Test Last Name",
            NameLast_Visible = true,
            Status = "Active",
            PostDate = DateTime.Now,
            UserPoints = 100,
            Type = "Regular",
            ProfileIntroduction = "Test introduction"
        };

        var userId = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.PrimarySid, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        var mockUser = new Members { MemberID = userId };
        _mockUserService.Setup(x => x.GetUserById(userId)).Returns(mockUser);
        _mockUserService.Setup(x => x.Update(It.IsAny<Members>())).Verifiable();

        // Act
        var result = _controller.UpdateUser(model);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockUserService.Verify(x => x.Update(It.IsAny<Members>()), Times.Once);
    }

    /// <summary>
    /// 注册用户
    /// </summary>
    [Fact]
    public void Register_ValidInput_ReturnsSuccessJson()
    {
        // Arrange
        var model = new MembersDto
        {
            Name = "Test Name",
            UserName = "testuser" + Guid.NewGuid().ToString("N"), // 确保用户名唯一
            Name_Visible = true,
            Name_Last = "Test Last Name",
            NameLast_Visible = true,
            Status = "Active",
            PostDate = DateTime.Now,
            UserPoints = 100,
            Type = "Regular",
            ProfileIntroduction = "Test introduction",
            Email = "test" + Guid.NewGuid().ToString("N") + "@example.com", // 确保邮箱唯一
            ParentMemberUserName = null
        };

        _mockUserService.Setup(x => x.Register(It.IsAny<Members>(), It.IsAny<string>(), It.IsAny<string>())).Returns((true, string.Empty));

        // Act
        var result = _controller.Register(model) as JsonResult;

        // Assert
        Assert.NotNull(result);
        var resultObject = result.Value.GetType().GetProperty("done").GetValue(result.Value);
        Assert.Equal(1, resultObject);
    }

    /// <summary>
    /// 验证邮箱
    /// </summary>
    [Fact]
    public void VerifyEmail_ValidCode_ReturnsSuccessJson()
    {
        // Arrange
        var code = "123456";
        var mockContact = new List<UserContact> { new UserContact { Verified = false } };
        _mockUserContactDataAccess.Setup(x => x.GetData(code, false, string.Empty, null)).Returns(mockContact);

        // Act
        var result = _controller.VerifyEmail(code) as JsonResult;

        // Assert
        Assert.NotNull(result);
        var resultObject = result.Value.GetType().GetProperty("done").GetValue(result.Value);
        Assert.Equal(1, resultObject);
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    [Fact]
    public void GetUserInfo_ValidId_ReturnsUserDto()
    {
        // Arrange
        var userId = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.PrimarySid, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        var mockUser = new Members { MemberID = userId };
        _mockUserService.Setup(x => x.GetUserById(userId)).Returns(mockUser);
        _mockMembersDataAccess.Setup(x => x.GetUserById(userId)).Returns(mockUser);
        var mockAddresses = new List<UserAddress>();
        _mockUserAddressDataAccess.Setup(x => x.GetData(null, userId)).Returns(mockAddresses);

        // Act
        var result = _controller.GetUserInfo(userId) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        var data = result.Value as UserDto;
        Assert.NotNull(data);
        Assert.Equal(userId, data.MemberID);
    }

    /// <summary>
    /// 创建地址
    /// </summary>
    [Fact]
    public void CreateUserAddress_ValidInput_ReturnsSuccessJson()
    {
        // Arrange
        var userId = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI";
        // Arrange
        var addressModel = new UserAddress
        {
            MemberID = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI",
            Address1 = "123 Test St",
            Address2 = "Apt 456",
            Address3 = "Building 789",
            AddressType = "Home",
            Description = "Test description",
            PostCode = "12345",
            PostDate = DateTime.Now,
            PublicPrivate = 1,
            RegionalCouncil = "Test Council",
            City = "Test City",
            State = "Test State",
            Country = 10000
        };
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.PrimarySid, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
        _mockUserAddressDataAccess.Setup(x => x.AddUserAddress(It.IsAny<UserAddress>())).Returns(1);

        // Act
        var result = _controller.CreateAddress(addressModel) as OkResult;

        // Assert
        Assert.NotNull(result);
    }

    /// <summary>
    /// 获取地址
    /// </summary>
    [Fact]
    public void GetAddress_ValidId_ReturnsAddressDto()
    {
        // Arrange
        var addressId = 11010;
        var mockAddress = new UserAddress
        {
            AddressID = addressId,
            MemberID = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI"
        };
        _mockUserService.Setup(x => x.GetUserAddressById(addressId)).Returns(mockAddress);

        // Act
        var result = _controller.GetAddressDetail(addressId) as JsonResult;

        // Assert
        Assert.NotNull(result);
        var data = result.Value.GetType().GetProperty("data").GetValue(result.Value) as UserAddress;
        Assert.NotNull(data);
    }

    /// <summary>
    /// 编辑地址
    /// </summary>
    [Fact]
    public void EditUserAddress_ValidInput_ReturnsSuccessJson()
    {
        // Arrange
        var addressId = 12010;
        var addressModel = new UserAddress
        {
            AddressID = addressId,
            MemberID = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI",
            Address1 = "Updated 123 Test St",
            Address2 = "Updated Apt 456",
            Address3 = "Building 789",
            AddressType = "Home",
            Description = "Test description",
            PostCode = "12345",
            PostDate = DateTime.Now,
            PublicPrivate = 1,
            RegionalCouncil = "Test Council",
            City = "Updated Test City",
            State = "Updated Test State",
            Country = 10000
        };
        var userId = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.PrimarySid, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
        _mockUserAddressDataAccess.Setup(x => x.UpdateUserAddress(It.IsAny<UserAddress>())).Returns(1);

        // Act
        var result = _controller.UpdateAddress(addressModel) as OkResult;

        // Assert
        Assert.NotNull(result);
    }

    /// <summary>
    /// 删除地址
    /// </summary>
    [Fact]
    public void DeleteUserAddress_ValidId_ReturnsOk()
    {
        // Arrange
        var addressId = 12010;
        var mockAddress = new UserAddress
        {
            AddressID = addressId,
            MemberID = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI"
        };
        _mockUserService.Setup(x => x.GetUserAddressById(addressId)).Returns(mockAddress);
        _mockUserAddressDataAccess.Setup(x => x.DeleteUserAddress(addressId)).Returns(1);

        // Act
        var result = _controller.DeleteUserAddress(addressId);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    /// <summary>
    /// 创建联系人
    /// </summary>
    [Fact]
    public void CreateUserContact_ValidInput_ReturnsSuccessJson()
    {
        // Arrange
        var contactModel = new UserContact
        {
            MemberID = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI",
            Notes = "Notes",
            ContactType = "Email",
            ContactDetail = "updatedtest@example.com",
            Verified = false,
            PublicPrivate = 1,
            PostDate = DateTime.Now,
        };
        var userId = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.PrimarySid, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
        _mockUserContactDataAccess.Setup(x => x.AddUserContact(It.IsAny<UserContact>(), null, null)).Returns(1);

        // Act
        var result = _controller.CreateConnect(contactModel) as OkResult;

        // Assert
        Assert.NotNull(result);

        contactModel = new UserContact
        {
            MemberID = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI",
            Notes = "Notes",
            ContactType = "Number",
            ContactDetail = "18111111111",
            Verified = false,
            PublicPrivate = 1,
            PostDate = DateTime.Now,
        };

        result = _controller.CreateConnect(contactModel) as OkResult;
        Assert.NotNull(result);
    }

    /// <summary>
    /// 修改联系人
    /// </summary>
    [Fact]
    public void UpdateUserContact_ValidInput_ReturnsSuccessJson()
    {
        // Arrange
        var contactModel = new UserContact
        {
            ContactID = 4,
            MemberID = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI",
            ContactDetail = "271520594@qq.com",
            ContactType = "Email",
            Notes = "YXM908",
            PostDate = DateTime.Now,
            PublicPrivate = 1,
            Verified = true
        };
        var userId = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.PrimarySid, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        _mockUserContactDataAccess.Setup(x => x.UpdateUserContact(It.IsAny<UserContact>())).Returns(1);

        // Act
        var result = _controller.UpdateConnect(contactModel) as OkResult;

        // Assert
        Assert.NotNull(result);
    }

    /// <summary>
    /// 删除联系人
    /// </summary>
    [Fact]
    public void DeleteUserContact_ValidId_ReturnsSuccessJson()
    {
        // Arrange
        var contactId = 8;
        var mockContact = new List<UserContact> { new UserContact { ContactID = contactId } };
        _mockUserContactDataAccess.Setup(x => x.GetData(string.Empty, null, string.Empty, contactId)).Returns(mockContact);
        _mockUserContactDataAccess.Setup(x => x.DeleteUserContact(contactId)).Returns(1);

        // Act
        var result = _controller.DeleteUserConnect(contactId) as OkResult;

        // Assert
        Assert.NotNull(result);
    }


    /// 注册 - 用户名长度边界值测试（最小长度）
    /// </summary>
    [Fact]
    public void Register_MinUserNameLength_ReturnsSuccessJson()
    {
        // Arrange
        var model = new MembersDto
        {
            Name = "Test Name",
            UserName = "t",
            Name_Visible = true,
            Name_Last = "Test Last Name",
            NameLast_Visible = true,
            Status = "Active",
            PostDate = DateTime.Now,
            UserPoints = 100,
            Type = "Regular",
            ProfileIntroduction = "Test introduction",
            Email = "test" + Guid.NewGuid().ToString("N") + "@example.com",
            ParentMemberUserName = null
        };

        _mockUserService.Setup(x => x.Register(It.IsAny<Members>(), It.IsAny<string>(), It.IsAny<string>())).Returns((true, string.Empty));

        // Act
        var result = _controller.Register(model) as JsonResult;

        // Assert
        Assert.NotNull(result);
        var c = result.Value.GetType().GetProperty("done").GetValue(result.Value);
        Assert.Equal(1, c);
    }

    /// <summary>
    /// 注册 - 用户名长度边界值测试（最大长度）
    /// </summary>
    [Fact]
    public void Register_MaxUserNameLength_ReturnsSuccessJson()
    {
        // Arrange
        var maxLength = 150;
        var model = new MembersDto
        {
            Name = "Test Name",
            UserName = new string('a', maxLength),
            Name_Visible = true,
            Name_Last = "Test Last Name",
            NameLast_Visible = true,
            Status = "Active",
            PostDate = DateTime.Now,
            UserPoints = 100,
            Type = "Regular",
            ProfileIntroduction = "Test introduction",
            Email = "test" + Guid.NewGuid().ToString("N") + "@example.com",
            ParentMemberUserName = null
        };

        _mockUserService.Setup(x => x.Register(It.IsAny<Members>(), It.IsAny<string>(), It.IsAny<string>())).Returns((true, string.Empty));

        // Act
        var result = _controller.Register(model) as JsonResult;

        // Assert
        Assert.NotNull(result);
        var c = result.Value.GetType().GetProperty("done").GetValue(result.Value);
        Assert.Equal(1, c);
    }

    /// <summary>
    /// 注册 - 邮箱为空值测试
    /// </summary>
    [Fact]
    public void Register_EmptyEmail_ReturnsFailureJson()
    {
        // Arrange
        var model = new MembersDto
        {
            Name = "Test Name",
            UserName = "testuser" + Guid.NewGuid().ToString("N"),
            Name_Visible = true,
            Name_Last = "Test Last Name",
            NameLast_Visible = true,
            Status = "Active",
            PostDate = DateTime.Now,
            UserPoints = 100,
            Type = "Regular",
            ProfileIntroduction = "Test introduction",
            Email = "",
            ParentMemberUserName = null
        };

        _mockUserService.Setup(x => x.Register(It.IsAny<Members>(), It.IsAny<string>(), It.IsAny<string>())).Returns((false, "Email is required"));

        // Act
        var result = _controller.Register(model) as JsonResult;

        // Assert
        Assert.NotNull(result);
        var c = result.Value.GetType().GetProperty("done").GetValue(result.Value);
        Assert.Equal(0, c);
    }

    /// <summary>
    /// 更新用户 - 用户 ID 为空值测试
    /// </summary>
    [Fact]
    public void UpdateUser_EmptyUserId_ReturnsError()
    {
        // Arrange
        var model = new MembersDto
        {
            Name = "Test Name",
            UserName = "testuser",
            Name_Visible = true,
            Name_Last = "Test Last Name",
            NameLast_Visible = true,
            Status = "Active",
            PostDate = DateTime.Now,
            UserPoints = 100,
            Type = "Regular",
            ProfileIntroduction = "Test introduction"
        };
        var userId = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.PrimarySid, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        var mockUser = new Members { MemberID = userId };
        _mockUserService.Setup(x => x.GetUserById(userId)).Returns(mockUser);
        // Act
        var result = _controller.UpdateUser(model);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    /// <summary>
    /// 获取用户信息 - 用户 ID 为空值测试
    /// </summary>
    [Fact]
    public void GetUserInfo_EmptyUserId_ReturnsNotFound()
    {
        var userId = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.PrimarySid, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        _mockUserService.Setup(x => x.GetUserById("")).Returns((Members)null);

        // Act
        var result = _controller.GetUserInfo("");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>
    /// 创建地址 - 地址 1 为空值测试
    /// </summary>
    [Fact]
    public void CreateUserAddress_EmptyAddress1_ReturnsSuccessJson()
    {
        // Arrange
        var addressModel = new UserAddress
        {
            MemberID = "US7QUIV8UIYS5UUBZIT30HURFHF5C3KAQDSS356EE2ZN76EXOBS9YKKTZQA5M0KI",
            Address1 = "",
            Address2 = "Apt 456",
            Address3 = "Building 789",
            AddressType = "Home",
            Description = "Test description",
            PostCode = "12345",
            PostDate = DateTime.Now,
            PublicPrivate = 1,
            RegionalCouncil = "Test Council",
            City = "Test City",
            State = "Test State",
            Country = 10000
        };
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.PrimarySid, "")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
        _mockUserAddressDataAccess.Setup(x => x.AddUserAddress(It.IsAny<UserAddress>())).Returns(1);

        // Act
        var result = _controller.CreateAddress(addressModel) as OkResult;

        // Assert
        Assert.NotNull(result);
    }

    /// <summary>
    /// 删除地址 - 地址 ID 为 0 测试
    /// </summary>
    [Fact]
    public void DeleteUserAddress_ZeroAddressId_ReturnsNotFound()
    {
        // Arrange
        var addressId = 0;
        _mockUserService.Setup(x => x.GetUserAddressById(addressId)).Returns((UserAddress)null);

        // Act
        var result = _controller.DeleteUserAddress(addressId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}