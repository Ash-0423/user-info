using Microsoft.Data.SqlClient;
using System.Data;
using UserInfoManager.Models;

namespace UserInfoManager.Data;

public class RelationshipDataAccess(AdoNetHelper _dbHelper) : IRelationshipDataAccess
{
    public int AddRelationship(Relationship user, SqlTransaction tran, SqlConnection conn)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@MemberID_Parent", user.MemberID_Parent),
            new SqlParameter("@MemberID_Child", user.MemberID_Child),
            new SqlParameter("@Notes", string.IsNullOrEmpty(user.Notes) ?(object) DBNull.Value : user.Notes),
            new SqlParameter("@Timestamp", user.Timestamp ?? (object)DBNull.Value)
        };

        return _dbHelper.ExecuteNonQuery("sp_Relationship_AddRelationship", CommandType.StoredProcedure, tran, conn, parameters.ToArray());
    }
}
