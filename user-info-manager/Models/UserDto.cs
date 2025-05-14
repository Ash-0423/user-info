namespace UserInfoManager.Models;

public class UserDto
{
    public string MemberID { get; set; }
    public string UserName { get; set; }
    public string? Name { get; set; }
    public bool? Name_Visible { get; set; }
    public string? Name_Last { get; set; }
    public bool? NameLast_Visible { get; set; }
    public string? ProfileIntroduction { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public int? UserPoints { get; set; }
    public DateTime? PostDate { get; set; }
    public List<UserAddressDto> Address { get; set; }
}

public class MembersDto
{
    public string? MemberID { get; set; }
    public string UserName { get; set; }
    public string? Name { get; set; }
    public bool? Name_Visible { get; set; }
    public string? Name_Last { get; set; }
    public bool? NameLast_Visible { get; set; }
    public string? ProfileIntroduction { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public int? UserPoints { get; set; }
    public DateTime? PostDate { get; set; }
    public string? Email { get; set; }
    public string? ParentMemberUserName { get; set; }
}


public class UpdateUserDto
{
    public string UserName { get; set; }
    public string? Name { get; set; }
    public bool? Name_Visible { get; set; }
    public string? Name_Last { get; set; }
    public bool? NameLast_Visible { get; set; }
    public string? ProfileIntroduction { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public int? UserPoints { get; set; }
    public DateTime? PostDate { get; set; }
    public List<UserAddressDto> Addresses { get; set; }
}

public class UserAddressDto
{
    public long? AddressID { get; set; }
    public string MemberID { get; set; }
    public string AddressType { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? Address3 { get; set; }
    public string? City { get; set; }
    public string? PostCode { get; set; }
    public string? RegionalCouncil { get; set; }
    public string? State { get; set; }
    public long? Country { get; set; }
    public int? PublicPrivate { get; set; }
    public DateTime? PostDate { get; set; }
}

public class LoginDto
{
    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; }
}