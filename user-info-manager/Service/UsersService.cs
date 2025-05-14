using Microsoft.Data.SqlClient;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Data;
using UserInfoManager.Data;
using UserInfoManager.Models;

namespace UserInfoManager.Service;

public class UsersService(AdoNetHelper _dbHelper,
    IMembersDataAccess _membersDataAccess,
    IUserContactDataAccess _userContactDataAccess,
    IUserAddressDataAccess _userAddressDataAccess,
    IRelationshipDataAccess _relationshipDataAccess) : IUsersService
{
    // 登录验证方法
    public (Members, UserContact) Authenticate(string email)
    {
        string sql = "[dbo].[sp_Members_Authenticate]";
        SqlParameter parameter = new SqlParameter("@Email", email);

        using (SqlDataReader reader = _dbHelper.ExecuteReader(sql, CommandType.StoredProcedure, parameter))
        {
            if (reader.Read())
            {
                UserContact contact = MapToUserContact(reader);
                Members member = MapToMembers(reader);
                return (member, contact);
            }
            return (null, null);
        }
    }

    // 注册方法
    public (bool, string) Register(Members user, string email, string parentUserName)
    {
        try
        {
            // 检查用户名是否已存在
            if (UserExistsByUserName(user.UserName) != null)
                return (false, "registration failed, the userName is already taken.");

            Members parentUser = null;
            if (!string.IsNullOrEmpty(parentUserName))
            {
                parentUser = UserExistsByUserName(parentUserName);
                if (parentUser == null)
                    return (false, "registration failed, the parentUserName is already taken.");
            }

            // 检查邮箱是否已存在
            if (EmailExists(email))
                return (false, "registration failed, the email is already taken.");

            string memberId;
            do
            {
                memberId = GenerateRandomString(64);
            } while (MemberIdExists(memberId));

            var emailCode = GenerateRandomString(6);

            // 开始事务
            using (SqlConnection connection = new SqlConnection(_dbHelper.ConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 插入用户
                    user.MemberID = memberId;
                    _membersDataAccess.AddUser(user, transaction, connection);

                    // 插入邮箱验证记录
                    UserContact contact = new UserContact
                    {
                        ContactDetail = email,
                        Verified = false,
                        ContactType = "Email",
                        MemberID = memberId,
                        Notes = emailCode,
                        PostDate = DateTime.Now
                    };

                    _userContactDataAccess.AddUserContact(contact, transaction, connection);

                    // 插入关系（如果有父用户）
                    if (parentUser != null)
                    {
                        Relationship relationship = new Relationship
                        {
                            MemberID_Parent = parentUser.MemberID,
                            MemberID_Child = memberId,
                            Timestamp = DateTime.Now,
                            Notes = emailCode
                        };

                        _relationshipDataAccess.AddRelationship(relationship, transaction, connection);
                    }


                    // 发送邮件
                    var apiKey = "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";
                    var client = new SendGridClient(apiKey);
                    var from = new EmailAddress("zhengdonghao0112@gmail.com", "zhengdonghao0112");
                    var to = new EmailAddress(email, email);
                    var subject = "Register Success";
                    var plainTextContent = $"Registration successful, your verification code is: {emailCode}， Please go to the opened page to complete the verification. Click on the link:http://170.64.235.76:8081/AccountView/VerifyEmailPage Enter the verification code for verification.";
                    var htmlContent = $"<p>Registration successful, your verification code is: {emailCode}， Please go to the opened page to complete the verification. Click on the link:http://170.64.235.76:8081/AccountView/VerifyEmailPage Enter the verification code for verification.</p>";
                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                    var result = client.SendEmailAsync(msg).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        // 提交事务
                        transaction.Commit();
                        return (true, string.Empty);
                    }
                    else
                    {
                        // 回滚事务
                        transaction.Rollback();
                        return (false, "Failed to send verification email");
                    }
                }
                catch (Exception ex)
                {
                    // 回滚事务
                    transaction.Rollback();
                    return (false, ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public Members GetUserById(string userId)
    {
        return _membersDataAccess.GetUserById(userId);
    }

    public UserAddress GetUserAddressById(int id)
    {
        return _userAddressDataAccess.GetData(id, string.Empty).First();
    }

    /// <summary>
    /// 修改用户
    /// </summary>
    /// <param name="user"></param>
    public void Update(Members user)
    {
        _membersDataAccess.UpdateUser(user);
    }

    // 辅助方法：检查用户名是否存在
    private Members UserExistsByUserName(string userName)
    {
        return _membersDataAccess.UserExistsByUserName(userName);
    }

    // 辅助方法：检查邮箱是否存在
    private bool EmailExists(string email)
    {
        string sql = "[dbo].[sp_Contact_CheckEmailExists]";
        SqlParameter parameter = new SqlParameter("@Email", email);

        object result = _dbHelper.ExecuteScalar(sql, CommandType.StoredProcedure, parameter);
        return Convert.ToInt32(result) > 0;
    }

    // 辅助方法：检查会员ID是否存在
    private bool MemberIdExists(string memberId)
    {
        string sql = "[dbo].[sp_Members_CheckMemberIdExists]";
        SqlParameter parameter = new SqlParameter("@MemberID", memberId);

        object result = _dbHelper.ExecuteScalar(sql, CommandType.StoredProcedure, parameter);
        return Convert.ToInt32(result) > 0;
    }

    // 辅助方法：数据映射
    private UserContact MapToUserContact(SqlDataReader reader)
    {
        return new UserContact
        {
            ContactID = Convert.ToInt32(reader["ContactID"]),
            MemberID = reader["MemberID"].ToString(),
            ContactType = reader["ContactType"].ToString(),
            ContactDetail = reader["ContactDetail"].ToString(),
            Verified = reader["Verified"] == DBNull.Value ? (bool?)null : Convert.ToInt32(reader["Verified"].ToString()) == 1,
            Notes = reader["Notes"] == DBNull.Value ? null : reader["Notes"].ToString(),
            PublicPrivate = reader["PublicPrivate"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["PublicPrivate"].ToString()),
            PostDate = reader["PostDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["PostDate"]
        };
    }

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

    // 生成随机字符串方法
    private string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        Random random = new Random();
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
