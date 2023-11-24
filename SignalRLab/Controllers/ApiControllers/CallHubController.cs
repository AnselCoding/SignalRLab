using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRLab.Hubs;
using System.Security.Claims;

namespace SignalRLab.Controllers.ApiControllers
{
    [ApiExplorerSettings(GroupName = "controllers")] //顯示於 Swagger 指定分頁中
    [Authorize(Policy = "PolicyForPath2")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CallHubController : ControllerBase
    {
        private readonly IHubContext<AllHub, IBaseHub> _hubContext;

        public CallHubController(IHubContext<AllHub, IBaseHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // GET: api/CallHub/SendMessage
        [HttpGet]
        public async Task<ActionResult> SendMessage(string message)
        {
            // 取 ClaimsPrincipal 使用者資料
            var userId = User.FindFirstValue("Id");
            await _hubContext.Clients.All.OnReceivePodcast(User.FindFirstValue("name"), message);
            return Ok(userId);
        }
    }
}
