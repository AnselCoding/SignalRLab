using Microsoft.AspNetCore.Authorization;
using SignalRLab.SignalRTimer;
using SignalRSwaggerGen.Attributes;
using SignalRSwaggerGen.Enums;
using System.Runtime.CompilerServices;

namespace SignalRLab.Hubs
{
    [SignalRHub(autoDiscover: AutoDiscover.MethodsAndParams, documentNames: new[] { "hubs" })] //顯示於 Swagger 指定分頁中
    [Authorize(Policy = "PolicyForPath2")]
    public class AllHub : BaseHub
    {
        private IDisconnectTimer _disconnectTimer;
        public AllHub(IDisconnectTimer disconnectTimer) 
        {
            _disconnectTimer = disconnectTimer;
        }

        /// <summary>
        /// 連線事件
        /// </summary>
        /// <returns></returns>
        [SignalRHidden]
        public override async Task OnConnectedAsync()
        {
            // 取 JWT token 中的 Claim info
            var userId = UserId;

            // 更新聊天內容，通知新連線
            await Clients.All.OnReceivePodcast("新連線 ID", userId);

            _disconnectTimer.Start();

            await base.OnConnectedAsync();

        }

        /// <summary>
        /// 離線事件
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        [SignalRHidden]
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _disconnectTimer.Stop();

            // 更新聊天內容，通知離線
            await Clients.All.OnReceivePodcast("已離線 ID", base.GetUserIdFromConnectionId(Context.ConnectionId));

            await base.OnDisconnectedAsync(exception);
        }

        [SignalRMethod(summary: "廣播發送訊息", description: "對應接收事件 OnReceivePodcast。", autoDiscover: AutoDiscover.Params)]
        public async Task SendMessageToAll(string message)
        {
            _disconnectTimer.Restart();

            // 將訊息傳送給所有連接的客戶端
            await Clients.All.OnReceivePodcast(UserId, message);            
        }

        public async Task SendObj(DynamicAttribute msgObj)
        {
            // 匿名函式 OK
            //await Clients.Caller.OnReceiveObj(new {name=$"{UserId}", age=42 });
            
            await Clients.Caller.OnReceiveObj(msgObj);
        }
    }
}

    