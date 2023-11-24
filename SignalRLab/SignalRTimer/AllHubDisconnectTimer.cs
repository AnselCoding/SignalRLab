
using Microsoft.AspNetCore.SignalR;
using SignalRLab.Hubs;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace SignalRLab.SignalRTimer
{

    public class AllHubDisconnectTimer : DisconnectTimerBase, IDisconnectTimer
    {
        private readonly IHubContext<AllHub, IBaseHub> _hubContext;

        public AllHubDisconnectTimer(ILogger<AllHubDisconnectTimer> logger, IHubContext<AllHub, IBaseHub> hubContext, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _logger = logger;
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            NotifyMin = _configuration.GetSection("SignalR:MemberKeepNotifyMin").Get<int>();
            ConnectionExpireMin = _configuration.GetSection("SignalR:MemberConnectionExpireMin").Get<int>();
        }

        protected override void DoActionA()
        {
            Stop();

            _logger.LogInformation("Doing Action A...");
            _hubContext.Clients.All.OnReceivePodcast(_userId, "Doing Action A...").Wait();

            // 設定計時器，每15秒執行一次 DoActionB 方法
            StartTimer(ConnectionExpireMin, DoActionB);
        }

        protected override void DoActionB()
        {
            _logger.LogInformation("Doing Action B...");
            // SignalR的設計，需要從前端才能正常斷開連線
            _hubContext.Clients.User(_userId).OnDisconnect().Wait();

            Stop();
        }

    }

}
