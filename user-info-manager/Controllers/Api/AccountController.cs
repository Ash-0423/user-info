using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserInfoManager.Data;
using UserInfoManager.Models;
using UserInfoManager.Service;

namespace UserInfoManager.Controllers.Api;

[ApiController]
[Route("module/[controller]")]
public class AccountController(IUsersService _userService,
    IConfiguration _configuration,
    IMembersDataAccess membersDataAccess,
    IUserContactDataAccess userContactDataAccess,
    IUserAddressDataAccess userAddressDataAccess) : ControllerBase
{
    /// <summary>
    /// update user
    /// </summary>
    /// <returns></returns>
    [HttpPost("update-user")]
    public IActionResult UpdateUser([FromBody] MembersDto members)
    {
        var userId = User.FindFirst(ClaimTypes.PrimarySid).Value;
        var user = _userService.GetUserById(userId);
        user.Name = members.Name;
        user.UserName = members.UserName;
        user.Name_Visible = members.Name_Visible;
        user.PostDate = members.PostDate;
        user.Name_Last = members.Name_Last;
        user.NameLast_Visible = members.NameLast_Visible;
        user.UserPoints = members.UserPoints;
        user.Type = members.Type;
        user.Name_Last = members.Name_Last;
        user.Status = members.Status;
        user.ProfileIntroduction = members.ProfileIntroduction;
        _userService.Update(user);
        return Ok();
    }

    /// <summary>
    /// user register
    /// </summary>
    /// <param name="user"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpPost("register")]
    public IActionResult Register([FromBody] MembersDto model)
    {
        var result = _userService.Register(new Members
        {
            Name = model.Name,
            UserName = model.UserName,
            Name_Visible = model.Name_Visible,
            Name_Last = model.Name_Last,
            NameLast_Visible = model.NameLast_Visible,
            Status = model.Status,
            PostDate = model.PostDate,
            UserPoints = model.UserPoints,
            Type = model.Type,
            ProfileIntroduction = model.ProfileIntroduction
        }, model.Email, model.ParentMemberUserName);
        if (result.Item1)
            return new JsonResult(new { done = 1 });
        return new JsonResult(new { done = 0, msg = result.Item2 });
    }

    /// <summary>
    /// verify email
    /// </summary>
    /// <returns></returns>
    [HttpPost("verify-email")]
    public IActionResult VerifyEmail(string code)
    {
        var contact = userContactDataAccess.GetData(code, false, string.Empty, null);
        if (!contact.Any())
            return new JsonResult(new { done = 0, msg = "Code not found" });
        contact.First().Verified = true;
        userContactDataAccess.UpdateUserContact(contact.First());
        return new JsonResult(new { done = 1 });
    }

    /// <summary>
    /// delete address
    /// </summary>
    /// <returns></returns>
    [HttpDelete("user-address/{addressId}")]
    public IActionResult DeleteUserAddress(int addressId)
    {
        var address = _userService.GetUserAddressById(addressId);
        if (address == null) return NotFound();
        userAddressDataAccess.DeleteUserAddress(addressId);
        return Ok();
    }

    /// <summary>
    /// login
    /// </summary>
    /// <returns></returns>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto model)
    {
        var user = _userService.Authenticate(model.Email);//查询用户
        if (user.Item1 == null)
            return new JsonResult(new { done = 0, msg = "login fail，Please check your email！" });
        if (!user.Item2.Verified.Value)
            return new JsonResult(new { done = 0, msg = "login fail，Your email has not been verified yet. Please verify it before logging in." });
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, model.Email),
            new(ClaimTypes.PrimarySid, user.Item1.MemberID),
        };
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"]);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(expirationMinutes),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new JsonResult(new { done = 1, token = tokenString, data = user });
    }

    /// <summary>
    /// get user
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-user-info/{userId}")]
    public IActionResult GetUserInfo(string userId)
    {
        var user = _userService.GetUserById(userId);
        var userEmail = membersDataAccess.GetUserById(userId);
        var address = userAddressDataAccess.GetData(null, userId);
        if (user == null)
            return NotFound();
        return Ok(new UserDto
        {
            Name = user.Name,
            UserName = user.UserName,
            Name_Visible = user.Name_Visible,
            Status = user.Status,
            PostDate = user.PostDate,
            UserPoints = user.UserPoints,
            Type = user.Type,
            ProfileIntroduction = user.ProfileIntroduction,
            MemberID = user.MemberID,
            Address = [.. address.Select(a => new UserAddressDto
            {
                AddressID = a.AddressID,
                MemberID=a.MemberID,
                Address1 = a.Address1,
                Address2 = a.Address2,
                Address3 = a.Address3,
                AddressType = a.AddressType,
                City = a.City,
                Country = a.Country,
                PostCode = a.PostCode,
                PostDate = a.PostDate,
                PublicPrivate = a.PublicPrivate,
                RegionalCouncil = a.RegionalCouncil,
                State = a.State
            })]
        });
    }

    /// <summary>
    /// get address
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-address")]
    public IActionResult GetAddress()
    {
        var userId = User.FindFirst(ClaimTypes.PrimarySid).Value;
        var addresses = userAddressDataAccess.GetData(null, userId);
        return new JsonResult(new { done = 1, data = addresses });
    }
    /// <summary>
    /// get address
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-address/{id}")]
    public IActionResult GetAddressDetail(int id)
    {
        var address = _userService.GetUserAddressById(id);
        return new JsonResult(new { done = 1, data = address });
    }

    [HttpPost("update-address")]
    public IActionResult UpdateAddress([FromBody] UserAddress model)
    {
        model.MemberID = User.FindFirst(ClaimTypes.PrimarySid).Value;
        userAddressDataAccess.UpdateUserAddress(model);
        return Ok();
    }

    [HttpPost("create-address")]
    public IActionResult CreateAddress([FromBody] UserAddress model)
    {
        model.MemberID = User.FindFirst(ClaimTypes.PrimarySid).Value;
        userAddressDataAccess.AddUserAddress(model);
        return Ok();
    }
    /// <summary>
    /// get connect
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-connect")]
    public IActionResult GetConnect()
    {
        var userId = User.FindFirst(ClaimTypes.PrimarySid).Value;
        var connect = userContactDataAccess.GetData(string.Empty, null, userId, null);
        return new JsonResult(new { done = 1, data = connect });
    }
    /// <summary>
    /// get conn
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-connect/{id}")]
    public IActionResult GetConnectDetail(int id)
    {
        var conn = userContactDataAccess.GetData(string.Empty, null, string.Empty, id);
        return new JsonResult(new { done = 1, data = conn.First() });
    }

    [HttpPost("update-connect")]
    public IActionResult UpdateConnect([FromBody] UserContact model)
    {
        model.MemberID = User.FindFirst(ClaimTypes.PrimarySid).Value;
        model.Verified = false;
        userContactDataAccess.UpdateUserContact(model);
        return Ok();
    }

    [HttpPost("create-connect")]
    public IActionResult CreateConnect([FromBody] UserContact model)
    {
        model.MemberID = User.FindFirst(ClaimTypes.PrimarySid).Value;
        model.PostDate = DateTime.Now;
        model.Verified = false;
        userContactDataAccess.AddUserContact(model, null, null);
        return Ok();
    }

    /// <summary>
    /// delete connect
    /// </summary>
    /// <returns></returns>
    [HttpDelete("user-connect/{id}")]
    public IActionResult DeleteUserConnect(int id)
    {
        var address = userContactDataAccess.GetData(string.Empty, null, string.Empty, id);
        if (!address.Any()) return NotFound();
        userContactDataAccess.DeleteUserContact(id);
        return Ok();
    }

    /// <summary>
    /// get user
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-user")]
    public IActionResult GetUser()
    {
        var userId = User.FindFirst(ClaimTypes.PrimarySid).Value;
        var user = _userService.GetUserById(userId);
        return new JsonResult(new
        {
            done = 1,
            data = new UserDto
            {
                Name = user.Name,
                UserName = user.UserName,
                Name_Visible = user.Name_Visible,
                Status = user.Status,
                PostDate = user.PostDate,
                Name_Last = user.Name_Last,
                UserPoints = user.UserPoints,
                Type = user.Type,
                ProfileIntroduction = user.ProfileIntroduction,
                MemberID = user.MemberID
            }
        });
    }
    /// <summary>
    /// update user
    /// </summary>
    /// <returns></returns>
    [HttpPost("user-info/{userId}")]
    public IActionResult UpdateUserInfo(string userId, UpdateUserDto model)
    {
        _userService.Update(new Members
        {
            MemberID = userId,
            Name = model.Name,
            UserName = model.UserName,
            Name_Visible = model.Name_Visible,
            Name_Last = model.Name_Last,
            NameLast_Visible = model.NameLast_Visible,
            Status = model.Status,
            PostDate = model.PostDate,
            UserPoints = model.UserPoints,
            Type = model.Type,
            ProfileIntroduction = model.ProfileIntroduction
        });
        model.Addresses.Where(t => !t.AddressID.HasValue).Select(a => new UserAddress
        {
            MemberID = userId,
            Address1 = a.Address1,
            Address2 = a.Address2,
            Address3 = a.Address3,
            AddressType = a.AddressType,
            City = a.City,
            Country = a.Country,
            PostCode = a.PostCode,
            PostDate = a.PostDate,
            PublicPrivate = a.PublicPrivate,
            RegionalCouncil = a.RegionalCouncil,
            State = a.State
        }).ToList().ForEach(t => { t.PostDate = DateTime.Now; userAddressDataAccess.AddUserAddress(t); });
        model.Addresses.Where(t => t.AddressID.HasValue).Select(a => new UserAddress
        {
            AddressID = a.AddressID.Value,
            MemberID = userId,
            Address1 = a.Address1,
            Address2 = a.Address2,
            Address3 = a.Address3,
            AddressType = a.AddressType,
            City = a.City,
            Country = a.Country,
            PostCode = a.PostCode,
            PostDate = a.PostDate,
            PublicPrivate = a.PublicPrivate,
            RegionalCouncil = a.RegionalCouncil,
            State = a.State
        }).ToList().ForEach(t => { userAddressDataAccess.UpdateUserAddress(t); });
        return Ok();
    }
}
