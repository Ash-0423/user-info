using Microsoft.AspNetCore.Mvc;

namespace UserInfoManager.Controllers;

public class HomeController : Controller
{
    private readonly IConfiguration _configuration;
    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        ViewBag.ApiHost = _configuration.GetSection("Settings")["apihost"];
        return View();
    }

    public IActionResult Create()
    {
        ViewBag.ApiHost = _configuration.GetSection("Settings")["apihost"];
        return View();
    }

    public IActionResult CreateConnect()
    {
        ViewBag.ApiHost = _configuration.GetSection("Settings")["apihost"];
        return View();
    }

    public IActionResult Privacy()
    {
        ViewBag.ApiHost = _configuration.GetSection("Settings")["apihost"];
        return View();
    }

    public IActionResult Edit(int id)
    {
        ViewBag.ApiHost = _configuration.GetSection("Settings")["apihost"];
        return View();
    }

    public IActionResult EditConnect(int id)
    {
        ViewBag.ApiHost = _configuration.GetSection("Settings")["apihost"];
        return View();
    }

    /// <summary>
    /// 注销
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        //// 清除当前用户的身份认证信息
        //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        //// 清除任何之前登录时设置的临时信息
        //TempData.Clear();

        return Redirect("/accountview/login");
    }
}
