using Microsoft.Data.SqlClient;
using UserInfoManager.Models;

namespace UserInfoManager.Data;

public interface IUserContactDataAccess
{
    List<UserContact> GetData(string code, bool? verified, string memberId, int? contactID);
    int UpdateUserContact(UserContact user);
    int DeleteUserContact(int contactID);
    int AddUserContact(UserContact user, SqlTransaction tran, SqlConnection conn);
}
