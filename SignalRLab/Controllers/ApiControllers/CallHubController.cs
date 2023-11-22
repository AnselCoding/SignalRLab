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
        private readonly IHubContext<AllHub> _hubContext;

        public CallHubController(IHubContext<AllHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // GET: api/CallHub/SendMessage
        [HttpGet]
        public async Task<ActionResult> SendMessage(string message)
        {
            // 取 ClaimsPrincipal 使用者資料
            var userId = User.FindFirstValue("Id");
            await _hubContext.Clients.All.SendAsync("ReceivePodcast", User.FindFirstValue("name"), message);
            return Ok(userId);
        }
    }
}
