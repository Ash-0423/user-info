using Microsoft.Data.SqlClient;
using UserInfoManager.Models;

namespace UserInfoManager.Data;

public interface IRelationshipDataAccess
{
    int AddRelationship(Relationship user, SqlTransaction tran, SqlConnection conn);
}
