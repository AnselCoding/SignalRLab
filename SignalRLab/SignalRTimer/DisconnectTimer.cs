
using Microsoft.AspNetCore.SignalR;
using SignalRLab.Hubs;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace SignalRLab.SignalRTimer
{

    public class DisconnectTimer : IDisconnectTimer
    {
        private readonly ILogger<DisconnectTimer> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<AllHub, IBaseHub> _hubContext;
        private readonly IConfiguration _configuration;
        private readonly int NotifyMin;
        private readonly int ConnectionExpireMin;
        private static readonly ConcurrentDictionary<string, System.Timers.Timer> _userTimers = new ConcurrentDictionary<string, System.Timers.Timer>();
        private string _userId;


        public DisconnectTimer(ILogger<DisconnectTimer> logger, IHubContext<AllHub, IBaseHub> hubContext, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _logger = logger;
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            NotifyMin = _configuration.GetSection("SignalR:MemberKeepNotifyMin").Get<int>();
            ConnectionExpireMin = _configuration.GetSection("SignalR:MemberConnectionExpireMin").Get<int>();
        }
        public void Start()
        {
            _userId = _httpContextAccessor.HttpContext.User.FindFirstValue("name");

            _logger.LogInformation($"Timer started for user {_userId}.");
            //_hubContext.Clients.All.SendAsync("OnReceivePodcast", _userId, "Timer started.").Wait();
            StartTimer(NotifyMin, DoActionA);
        }

        private void StartTimer(int interval, Action action)
        {
            // 檢查是否已經存在計時器
            if (_userTimers.TryAdd(_userId, new System.Timers.Timer(interval * 1000)))
            {
                var timer = _userTimers[_userId];
                timer.Elapsed += (sender, args) => action();
                timer.Start();
            }
        }


        public void Restart()
        {
            _userId = _httpContextAccessor.HttpContext.User.FindFirstValue("name");

            // 重新計算時間
            // 先停止原計時器
            Stop();

            _logger.LogInformation("Timer restarted.");
            //_hubContext.Clients.All.SendAsync("OnReceivePodcast", _userId, "Timer restarted.").Wait();
            
            // 設定計時器，每5秒執行一次 DoActionA 方法
            Start();
        }

        public void Stop()
        {
            _userId = _httpContextAccessor.HttpContext.User.FindFirstValue("name");
            // 檢查是否存在計時器
            if (_userTimers.TryRemove(_userId, out var timer))
            {
                // 如果存在，則停止計時器
                timer.Stop();

                _logger.LogInformation($"Timer stopped for user {_userId}.");
            }
        }

        private void DoActionA()
        {
            Stop();

            _logger.LogInformation("Doing Action A...");
            _hubContext.Clients.All.OnReceivePodcast(_userId, "Doing Action A...").Wait();

            // 設定計時器，每15秒執行一次 DoActionB 方法
            StartTimer(ConnectionExpireMin, DoActionB);
        }

        private void DoActionB()
        {
            _logger.LogInformation("Doing Action B...");
            // SignalR的設計，需要從前端才能正常斷開連線
            _hubContext.Clients.User(_userId).OnDisconnect().Wait();

            Stop();
        }

    }

}
