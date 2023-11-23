using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;
using SignalRSwaggerGen.Enums;
using System.Security.Claims;

namespace SignalRLab.Hubs
{
    [SignalRHub(autoDiscover: AutoDiscover.MethodsAndParams, documentNames: new[] { "hubs" })] //顯示於 Swagger 指定分頁中
    [Authorize(Policy = "PolicyForPath2")]
    public class AllHub : BaseHub
    {
        /// <summary>
        /// 連線事件
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            // 取 JWT token 中的 Claim info
            var userId = UserId;

            // 更新聊天內容，通知新連線
            await Clients.All.SendAsync("ReceivePodcast", "新連線 ID", userId);
            
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
            // 更新聊天內容，通知離線
            await Clients.All.SendAsync("ReceivePodcast", "已離線 ID", base.GetUserIdFromConnectionId(Context.ConnectionId));

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageToAll( string message)
        {
            // 將訊息傳送給所有連接的客戶端
            await Clients.All.SendAsync("ReceivePodcast", UserId, message);
        }
    }
}

    