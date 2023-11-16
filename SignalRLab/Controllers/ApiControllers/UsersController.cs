using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SignalRLab.Controllers.ApiControllers
{
    [ApiExplorerSettings(GroupName = "controllers")]
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // GET: api/Users/AdminCheck
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AdminCheck()
        {
            // 取 ClaimsPrincipal 使用者資料
            //var name = User.FindFirstValue(ClaimTypes.Name);//取不到
            var name = User.FindFirstValue("name");
            return Ok(name);
        }

        // GET: api/Users/UserCheck
        [HttpGet]
        public ActionResult UserCheck()
        {
            // 取 ClaimsPrincipal 使用者資料
            var userId = User.FindFirstValue("Id");
            return Ok(userId);
        }
    }
}
