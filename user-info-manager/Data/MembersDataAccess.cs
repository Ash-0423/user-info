using Microsoft.Data.SqlClient;
using System.Data;
using UserInfoManager.Models;

namespace UserInfoManager.Data;

public class MembersDataAccess(AdoNetHelper _dbHelper): IMembersDataAccess
{
    /// <summary>
    /// 注册查询
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public Members UserExistsByUserName(string userName)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@UserName", userName)
        };

        using (SqlDataReader reader = _dbHelper.ExecuteReader("sp_Members_GetUserByUserName", CommandType.StoredProcedure, parameters.ToArray()))
        {
            if (reader.Read())
            {
                return MapToMembers(reader);
            }
            return null;
        }
    }
    // 获取用户信息
    public Members GetUserById(string userId)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@MemberID", userId)
        };

        using (SqlDataReader reader = _dbHelper.ExecuteReader("sp_Members_GetUserById", CommandType.StoredProcedure, parameters.ToArray()))
        {
            if (reader.Read())
            {
                return MapToMembers(reader);
            }
            return null;
        }
    }

    // 添加用户
    public int AddUser(Members user, SqlTransaction tran, SqlConnection conn)
    {
        string sql = "[dbo].[sp_Members_AddUser]";
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@MemberID", user.MemberID),
            new SqlParameter("@UserName", user.UserName),
            new SqlParameter("@Name", string.IsNullOrEmpty(user.Name)? (object)DBNull.Value:user.Name),
            new SqlParameter("@Name_Visible", user.Name_Visible ?? (object)DBNull.Value),
            new SqlParameter("@Name_Last", string.IsNullOrEmpty(user.Name_Last)? (object)DBNull.Value:user.Name_Last),
            new SqlParameter("@NameLast_Visible", user.NameLast_Visible ?? (object)DBNull.Value),
            new SqlParameter("@ProfileIntroduction", string.IsNullOrEmpty(user.ProfileIntroduction) ?(object) DBNull.Value : user.ProfileIntroduction),
            new SqlParameter("@Type", string.IsNullOrEmpty(user.Type) ?(object) DBNull.Value : user.Type),
            new SqlParameter("@Status", string.IsNullOrEmpty(user.Status) ?(object) DBNull.Value : user.Status),
            new SqlParameter("@UserPoints", user.UserPoints ?? (object)DBNull.Value),
            new SqlParameter("@PostDate", user.PostDate ?? (object)DBNull.Value)
        };

        return _dbHelper.ExecuteNonQuery(sql, CommandType.StoredProcedure, tran, conn, parameters.ToArray());
    }

    // 更新用户
    public int UpdateUser(Members user)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@MemberID", user.MemberID),
            new SqlParameter("@UserName", user.UserName),
            new SqlParameter("@Name", string.IsNullOrEmpty(user.Name) ?(object) DBNull.Value : user.Name),
            new SqlParameter("@Name_Visible", user.Name_Visible ?? (object)DBNull.Value),
            new SqlParameter("@Name_Last", string.IsNullOrEmpty(user.Name_Last) ?(object) DBNull.Value : user.Name_Last),
            new SqlParameter("@NameLast_Visible", user.NameLast_Visible ?? (object)DBNull.Value),
            new SqlParameter("@ProfileIntroduction", string.IsNullOrEmpty(user.ProfileIntroduction) ?(object) DBNull.Value : user.ProfileIntroduction),
            new SqlParameter("@Type", string.IsNullOrEmpty(user.Type) ?(object) DBNull.Value : user.Type),
            new SqlParameter("@Status", string.IsNullOrEmpty(user.Status) ?(object) DBNull.Value : user.Status),
            new SqlParameter("@UserPoints", user.UserPoints ?? (object)DBNull.Value),
            new SqlParameter("@PostDate", user.PostDate ?? (object)DBNull.Value)
        };

        return _dbHelper.ExecuteNonQuery("sp_Members_UpdateUser", CommandType.StoredProcedure, null, null, parameters.ToArray());
    }

    // 数据映射方法
    private Members MapToMembers(SqlDataReader reader)
    {
        return new Members
        {
            MemberID = reader["MemberID"].ToString(),
            UserName = reader["UserName"].ToString(),
            Name = reader["Name"] == DBNull.Value ? null : reader["Name"].ToString(),
            Name_Visible = reader["Name_Visible"] == DBNull.Value ? (bool?)null : (bool)reader["Name_Visible"],
            Name_Last = reader["Name_Last"] == DBNull.Value ? null : reader["Name_Last"].ToString(),
            NameLast_Visible = reader["NameLast_Visible"] == DBNull.Value ? (bool?)null : (bool)reader["NameLast_Visible"],
            ProfileIntroduction = reader["ProfileIntroduction"] == DBNull.Value ? null : reader["ProfileIntroduction"].ToString(),
            Type = reader["Type"] == DBNull.Value ? null : reader["Type"].ToString(),
            Status = reader["Status"] == DBNull.Value ? null : reader["Status"].ToString(),
            UserPoints = reader["UserPoints"] == DBNull.Value ? (int?)null : (int)reader["UserPoints"],
            PostDate = reader["PostDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["PostDate"]
        };
    }
}
