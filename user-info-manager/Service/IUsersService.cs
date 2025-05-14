using UserInfoManager.Models;

namespace UserInfoManager.Service;

public interface IUsersService
{
    (Members, UserContact) Authenticate(string email);
    (bool, string) Register(Members user, string email, string parentUserName);
    Members GetUserById(string userId);
    UserAddress GetUserAddressById(int id);
    void Update(Members user);
}
