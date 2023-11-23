using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SignalRLab.Hubs;
using System.Security.Claims;

namespace SignalRLab.SignalRTimer
{
    public class DisconnectTimer<T> : IDisconnectTimer<T> where T : Hub
    {
        private readonly HttpContext _httpContext;
        private readonly IHubContext<T> _hubContext;
        private readonly IConfiguration _configuration;
        private bool _run = false;

        public DisconnectTimer(IHubContext<T> hubContext, IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _httpContext = contextAccessor.HttpContext;
            _hubContext = hubContext;
            _configuration = configuration;
        }

        public string GetUserName()
        {
            return _httpContext.User.FindFirstValue("name");
        }

        public async Task Start()
        {
            _run = true;
            while (_run)
            {
                var testSecond = 5;

                var notifyMin = _configuration.GetSection("SignalR:MemberKeepNotifyMin").Get<int>();
                var connectionExpireMin = _configuration.GetSection("SignalR:MemberConnectionExpireMin").Get<int>();
                var userId = _httpContext.User.FindFirstValue("name");

                // 提示通知
                //await Task.Delay(notifyMin * 1000 * 60);
                await Task.Delay(testSecond * 1000);
                //await _hubContext.Clients.User(userId).SendAsync("ReceivePodcast", $"--{userId}--", $"注意將於{connectionExpireMin}分鐘後踢出");
                await _hubContext.Clients.User(userId).SendAsync("ReceivePodcast", $"--{userId}--", $"注意將於{testSecond}秒後踢出");
                
                // 執行踢出(SignalR 斷線)
                //await Task.Delay(connectionExpireMin * 1000 * 60);
                await Task.Delay(testSecond * 1000);
                await _hubContext.Clients.User(userId).SendAsync("ReceivePodcast", $"--{userId}--", $"踢出");

            }
        }
        public async void Resrart()
        {
            _run = false;
            await Start();
        }
        public void Stop()
        {
            _run = false;
        }
    }
}
