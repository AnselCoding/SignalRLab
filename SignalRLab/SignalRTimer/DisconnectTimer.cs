
using Microsoft.AspNetCore.SignalR;
using SignalRLab.Hubs;
using System.Security.Claims;

namespace SignalRLab.SignalRTimer
{

    public class DisconnectTimer : IHostedService, IDisposable
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<AllHub> _hubContext;
        private Timer _timerA;
        private Timer _timerB;
        private bool _isRunning;

        public DisconnectTimer(IHubContext<AllHub> hubContext, IHttpContextAccessor httpContextAccessor)
        {
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // 不自動啟動計時器，等待使用者觸發
            _isRunning = false;

            return Task.CompletedTask;
        }

        public async Task StartTimer()
        {
            if (!_isRunning)
            {
                // 設定計時器A，每5秒執行一次 DoActionA 方法
                _timerA = new Timer(DoActionA, null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);

                // 設定計時器B，每15秒執行一次 DoActionB 方法
                _timerB = new Timer(DoActionB, null, TimeSpan.FromSeconds(15), Timeout.InfiniteTimeSpan);

                _isRunning = true;
                Console.WriteLine("Timer started.");
                await _hubContext.Clients.All.SendAsync("ReceivePodcast", _httpContextAccessor.HttpContext.User.FindFirstValue("name"), "Timer started.");
            }
            else
            {
                Console.WriteLine("Timer is already running.");
            }
        }
        public async Task RestartTimer()
        {
            // 如果計時器正在運行，先停止它
            if (_isRunning)
            {
                // 停止計時器A
                _timerA?.Change(Timeout.Infinite, 0);
                // 停止計時器B
                _timerB?.Change(Timeout.Infinite, 0);
                _isRunning = false;
            }

            // 然後重新啟動計時器
            StartTimer();
            Console.WriteLine("Timer restarted.");
            await _hubContext.Clients.All.SendAsync("ReceivePodcast", _httpContextAccessor.HttpContext.User.FindFirstValue("name"), "Timer restarted.");
        }

        public void StopTimer()
        {
            // 停止計時器A
            _timerA?.Change(Timeout.Infinite, 0);
            // 停止計時器B
            _timerB?.Change(Timeout.Infinite, 0);
            _isRunning = false;

            Console.WriteLine("Timers stopped.");

        }

        private async void DoActionA(object state)
        {
            Console.WriteLine("Doing Action A...");
            await _hubContext.Clients.All.SendAsync("ReceivePodcast", _httpContextAccessor.HttpContext.User.FindFirstValue("name"), "Doing Action A...");            
        }

        private async void DoActionB(object state)
        {
            Console.WriteLine("Doing Action B...");
            await _hubContext.Clients.All.SendAsync("ReceivePodcast", _httpContextAccessor.HttpContext.User.FindFirstValue("name"), "Doing Action B...");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // 停止計時器A
            _timerA?.Change(Timeout.Infinite, 0);

            // 停止計時器B
            _timerB?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // 釋放計時器A
            _timerA?.Dispose();

            // 釋放計時器B
            _timerB?.Dispose();
        }
    }

}
