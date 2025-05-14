using Microsoft.Data.SqlClient;
using System.Data;
using UserInfoManager.Models;

namespace UserInfoManager.Data;

public class LookupsDataAccess(AdoNetHelper _dbHelper)
{
    public List<Lookups> GetData(string? keyword, string? lookupType)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@Keyword",string.IsNullOrEmpty(keyword) ?(object) DBNull.Value : keyword),
            new SqlParameter("@LookupType",string.IsNullOrEmpty(lookupType) ?(object) DBNull.Value : lookupType)
        };

        List<Lookups> data = new List<Lookups>();
        using (SqlDataReader reader = _dbHelper.ExecuteReader("sp_Lookups_GetData", CommandType.StoredProcedure, parameters.ToArray()))
        {
            while (reader.Read())
            {
                data.Add(MapToLookups(reader));
            }
            return data;
        }
    }
    private Lookups MapToLookups(SqlDataReader reader)
    {
        return new Lookups
        {
            LookupID = Convert.ToInt32(reader["LookupID"]),
            LookupSrc = char.Parse(reader["LookupSrc"].ToString()),
            LookupType = reader["LookupType"].ToString(),
            LookupCode = reader["LookupCode"].ToString(),
            Description = reader["Description"].ToString()
        };
    }
}
