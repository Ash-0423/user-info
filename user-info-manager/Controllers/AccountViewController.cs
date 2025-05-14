using Microsoft.AspNetCore.Mvc;

namespace UserInfoManager.Controllers;

public class AccountViewController(IConfiguration _configuration) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        ViewBag.ApiHost = _configuration.GetSection("Settings")["apihost"];
        return View();
    }

    /// <summary>
    /// 注册页面
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Register()
    {
        ViewBag.ApiHost = _configuration.GetSection("Settings")["apihost"];
        return View();
    }

    /// <summary>
    /// 验证邮箱页面
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult VerifyEmailPage()
    {
        ViewBag.ApiHost = _configuration.GetSection("Settings")["apihost"];
        return View();
    }
}
