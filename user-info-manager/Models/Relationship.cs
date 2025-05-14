namespace UserInfoManager.Models;

public class Relationship
{
    public long RelationshipNo { get; set; }
    public string MemberID_Parent { get; set; }
    public string MemberID_Child { get; set; }
    public string? Notes { get; set; }
    public DateTime? Timestamp { get; set; }
}
