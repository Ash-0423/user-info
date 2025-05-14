namespace UserInfoManager.Models;

public class UserContact
{
    public long ContactID { get; set; }
    public string? MemberID { get; set; }
    public string ContactType { get; set; }
    public string? ContactDetail { get; set; }
    public bool? Verified { get; set; }
    public string? Notes { get; set; }
    public int? PublicPrivate { get; set; }
    public DateTime? PostDate { get; set; }
}
