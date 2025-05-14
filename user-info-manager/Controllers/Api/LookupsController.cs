using Microsoft.AspNetCore.Mvc;
using UserInfoManager.Data;

namespace UserInfoManager.Controllers.Api;

[ApiController]
[Route("module/[controller]")]
public class LookupsController(LookupsDataAccess lookupsDataAccess) : ControllerBase
{
    /// <summary>
    /// get address
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-lookups")]
    public IActionResult GetAddress(string? keyword, string? lookupType)
    {
        return new JsonResult(new
        {
            done = 1,
            data = lookupsDataAccess.GetData(keyword, lookupType)
        });
    }
}
