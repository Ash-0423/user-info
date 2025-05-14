using Microsoft.Data.SqlClient;
using System.Data;
using UserInfoManager.Models;

namespace UserInfoManager.Data;

public class UserAddressDataAccess(AdoNetHelper _dbHelper) : IUserAddressDataAccess
{
    public int AddUserAddress(UserAddress user)
    {
        string sql = "[dbo].[sp_Address_AddUserAddress]";

        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@MemberID", user.MemberID),
            new SqlParameter("@AddressType", user.AddressType),
            new SqlParameter("@Address1", string.IsNullOrEmpty(user.Address1) ?(object) DBNull.Value : user.Address1),
            new SqlParameter("@Address2", string.IsNullOrEmpty(user.Address2) ?(object) DBNull.Value : user.Address2),
            new SqlParameter("@Address3", string.IsNullOrEmpty(user.Address3) ?(object) DBNull.Value : user.Address3    ),
            new SqlParameter("@City", string.IsNullOrEmpty(user.City) ?(object) DBNull.Value : user.City),
            new SqlParameter("@PostCode", string.IsNullOrEmpty(user.PostCode) ?(object) DBNull.Value : user.PostCode),
            new SqlParameter("@RegionalCouncil", string.IsNullOrEmpty(user.RegionalCouncil) ?(object) DBNull.Value : user.RegionalCouncil),
            new SqlParameter("@State", string.IsNullOrEmpty(user.State) ?(object) DBNull.Value : user.State),
            new SqlParameter("@Country", user.Country ?? (object)DBNull.Value),
            new SqlParameter("@PublicPrivate", user.PublicPrivate ?? (object)DBNull.Value),
            new SqlParameter("@PostDate", user.PostDate ?? (object)DBNull.Value)
        };

        return _dbHelper.ExecuteNonQuery(sql, CommandType.StoredProcedure, null, null, parameters.ToArray());
    }

    public int UpdateUserAddress(UserAddress user)
    {
        string sql = "[dbo].[sp_Address_UpdateUserAddress]";

        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@AddressID", user.AddressID),
            new SqlParameter("@MemberID", user.MemberID),
            new SqlParameter("@AddressType", user.AddressType),
            new SqlParameter("@Address1", string.IsNullOrEmpty(user.Address1) ?(object) DBNull.Value : user.Address1),
            new SqlParameter("@Address2",   string.IsNullOrEmpty(user.Address2) ?(object) DBNull.Value : user.Address2  ),
            new SqlParameter("@Address3", string.IsNullOrEmpty(user.Address3) ?(object) DBNull.Value : user.Address3),
            new SqlParameter("@City", string.IsNullOrEmpty(user.City) ?(object) DBNull.Value : user.City),
            new SqlParameter("@PostCode", string.IsNullOrEmpty(user.PostCode) ?(object) DBNull.Value : user.PostCode),
            new SqlParameter("@RegionalCouncil", string.IsNullOrEmpty(user.RegionalCouncil) ?(object) DBNull.Value : user.RegionalCouncil),
            new SqlParameter("@State", string.IsNullOrEmpty(user.State) ?(object) DBNull.Value : user.State),
            new SqlParameter("@Country", user.Country ?? (object)DBNull.Value),
            new SqlParameter("@PublicPrivate", user.PublicPrivate ?? (object)DBNull.Value),
            new SqlParameter("@PostDate", user.PostDate ?? (object)DBNull.Value)
        };

        return _dbHelper.ExecuteNonQuery(sql, CommandType.StoredProcedure, null, null, parameters.ToArray());
    }

    public List<UserAddress> GetData(int? id, string memberId)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@AddressID", id ?? (object)DBNull.Value),
            new SqlParameter("@MemberID", string.IsNullOrEmpty(memberId)? (object)DBNull.Value:memberId)
        };

        List<UserAddress> data = new List<UserAddress>();
        using (SqlDataReader reader = _dbHelper.ExecuteReader("sp_Address_GetData", CommandType.StoredProcedure, parameters.ToArray()))
        {
            while (reader.Read())
            {
                data.Add(MapToUserAddress(reader));
            }
            return data;
        }
    }
    public int DeleteUserAddress(int addressID)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@AddressID", addressID)
        };

        return _dbHelper.ExecuteNonQuery("sp_Address_DeleteUserAddress", CommandType.StoredProcedure, null, null, parameters.ToArray());
    }
    private UserAddress MapToUserAddress(SqlDataReader reader)
    {
        return new UserAddress
        {
            AddressID = Convert.ToInt32(reader["AddressID"]),
            MemberID = reader["MemberID"].ToString(),
            AddressType = reader["AddressType"].ToString(),
            Address1 = reader["Address1"].ToString(),
            Address2 = reader["Address2"].ToString(),
            Address3 = reader["Address3"].ToString(),
            City = reader["City"].ToString(),
            PostCode = reader["PostCode"].ToString(),
            RegionalCouncil = reader["RegionalCouncil"].ToString(),
            State = reader["State"].ToString(),
            Country = reader["Country"] == DBNull.Value ? null : long.Parse(reader["Country"].ToString()),
            PublicPrivate = reader["PublicPrivate"] == DBNull.Value ? null : int.Parse(reader["PublicPrivate"].ToString()),
            PostDate = reader["PostDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["PostDate"],
            Description = reader["Description"].ToString(),
        };
    }
}
