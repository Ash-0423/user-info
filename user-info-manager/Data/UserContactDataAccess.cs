using Microsoft.Data.SqlClient;
using System.Data;
using UserInfoManager.Models;

namespace UserInfoManager.Data;

public class UserContactDataAccess(AdoNetHelper _dbHelper) : IUserContactDataAccess
{
    public List<UserContact> GetData(string code, bool? verified, string memberId, int? contactID)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@Code", string.IsNullOrEmpty(code) ?(object) DBNull.Value : code),
            new SqlParameter("@Verified", verified ?? (object)DBNull.Value),
            new SqlParameter("@MemberID", string.IsNullOrEmpty(memberId)? (object)DBNull.Value:memberId),
            new SqlParameter("@ContactID", contactID ?? (object)DBNull.Value)
        };

        List<UserContact> data = new List<UserContact>();
        using (SqlDataReader reader = _dbHelper.ExecuteReader("sp_Contact_GetData", CommandType.StoredProcedure, parameters.ToArray()))
        {
            while (reader.Read())
            {
                data.Add(MapToUserContact(reader));
            }
            return data;
        }
    }
    private UserContact MapToUserContact(SqlDataReader reader)
    {
        return new UserContact
        {
            ContactID = (long)reader["ContactID"],
            MemberID = reader["MemberID"].ToString(),
            ContactType = reader["ContactType"].ToString(),
            ContactDetail = reader["ContactDetail"] == DBNull.Value ? (string?)null : (string)reader["ContactDetail"],
            Verified = reader["Verified"] == DBNull.Value ? null : reader["Verified"].ToString() == "1",
            Notes = reader["Notes"] == DBNull.Value ? (string?)null : (string)reader["Notes"],
            PublicPrivate = reader["PublicPrivate"] == DBNull.Value ? null : int.Parse(reader["PublicPrivate"].ToString()),
            PostDate = reader["PostDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["PostDate"]
        };
    }

    public int UpdateUserContact(UserContact user)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@ContactID", user.ContactID),
            new SqlParameter("@MemberID", user.MemberID),
            new SqlParameter("@ContactType", user.ContactType),
            new SqlParameter("@ContactDetail", string.IsNullOrEmpty(user.ContactDetail) ?(object) DBNull.Value : user.ContactDetail),
            new SqlParameter("@Verified", user.Verified ?? (object)DBNull.Value),
            new SqlParameter("@Notes", string.IsNullOrEmpty(user.Notes)? (object)DBNull.Value:user.Notes),
            new SqlParameter("@PublicPrivate", user.PublicPrivate ?? (object)DBNull.Value)
        };

        return _dbHelper.ExecuteNonQuery("sp_Contact_UpdateUserContact", CommandType.StoredProcedure, null, null, parameters.ToArray());
    }

    public int DeleteUserContact(int contactID)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@ContactID", contactID)
        };

        return _dbHelper.ExecuteNonQuery("sp_Contact_DeleteUserContact", CommandType.StoredProcedure, null, null, parameters.ToArray());
    }

    public int AddUserContact(UserContact user, SqlTransaction tran, SqlConnection conn)
    {
        string sql = "[dbo].[sp_Contact_AddUserContact]";

        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@MemberID", user.MemberID),
            new SqlParameter("@ContactType", user.ContactType),
            new SqlParameter("@ContactDetail", string.IsNullOrEmpty(user.ContactDetail) ?(object) DBNull.Value : user.ContactDetail),
            new SqlParameter("@Verified", user.Verified ?? (object)DBNull.Value),
            new SqlParameter("@Notes", string.IsNullOrEmpty(user.Notes) ?(object) DBNull.Value : user.Notes),
            new SqlParameter("@PublicPrivate", user.PublicPrivate ?? (object)DBNull.Value),
            new SqlParameter("@PostDate", user.PostDate ?? (object)DBNull.Value)
        };

        return _dbHelper.ExecuteNonQuery(sql, CommandType.StoredProcedure, tran, conn, parameters.ToArray());
    }
}
