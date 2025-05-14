using Microsoft.Data.SqlClient;
using UserInfoManager.Models;

namespace UserInfoManager.Data;

public interface IMembersDataAccess
{
    Members UserExistsByUserName(string userName);
    Members GetUserById(string userId);
    int AddUser(Members user, SqlTransaction tran, SqlConnection conn);
    int UpdateUser(Members user);

}
