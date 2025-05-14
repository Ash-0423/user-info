using UserInfoManager.Models;

namespace UserInfoManager.Data;

public interface IUserAddressDataAccess
{
    int AddUserAddress(UserAddress user);
    int UpdateUserAddress(UserAddress user);
    List<UserAddress> GetData(int? id, string memberId);
    int DeleteUserAddress(int addressID);

}
