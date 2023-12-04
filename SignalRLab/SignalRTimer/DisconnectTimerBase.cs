using Microsoft.AspNetCore.SignalR;
using SignalRLab.Hubs;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace SignalRLab.SignalRTimer
{
    public class DisconnectTimerBase
    {
        protected static ConcurrentDictionary<string, System.Timers.Timer> _userTimers = new ConcurrentDictionary<string, System.Timers.Timer>();
        protected IConfiguration _configuration;
        protected IHttpContextAccessor _httpContextAccessor;
        protected ILogger<DisconnectTimerBase> _logger;
        protected int ConnectionExpireMin;
        protected int NotifyMin;
        protected string _userId
        {
            get { return _httpContextAccessor.HttpContext.User.FindFirstValue("UserId"); }
        }


        public void Restart()
        {
            // 重新計算時間
            // 先停止原計時器
            Stop();

            _logger.LogInformation("Timer restarted.");
            //_hubContext.Clients.All.SendAsync("OnReceivePodcast", _userId, "Timer restarted.").Wait();

            // 設定計時器，每5秒執行一次 DoActionA 方法
            Start();
        }
        public void Start()
        {
            _logger.LogInformation($"Timer started for user {_userId}.");
            //_hubContext.Clients.All.SendAsync("OnReceivePodcast", _userId, "Timer started.").Wait();
            StartTimer(NotifyMin, DoActionA);
        }

        public void Stop()
        {
            // 檢查是否存在計時器
            if (_userTimers.TryRemove(_userId, out var timer))
            {
                // 如果存在，則停止計時器
                timer.Stop();

                _logger.LogInformation($"Timer stopped for user {_userId}.");
            }
        }

        protected void StartTimer(int minute, Action action)
        {
            // 檢查是否已經存在計時器
            if (_userTimers.TryAdd(_userId, new System.Timers.Timer(minute * 1000 * 60)))
            {
                var timer = _userTimers[_userId];
                timer.Elapsed += (sender, args) => action();
                timer.Start();
            }
            else
            {
                Restart();
            }
        }

        protected virtual void DoActionA() { }

        protected virtual void DoActionB() { }
    }
}