using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;
using SignalRSwaggerGen.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;

namespace SignalRLab.Hubs
{
    [SignalRHub(autoDiscover: AutoDiscover.MethodsAndParams, documentNames: new[] { "hubs" })]
    [Authorize]
    public class FirstSignalRHub : Hub
    {
        // 存放自訂義 userId 與 ConnectionId 的配對
        private static readonly ConcurrentDictionary<string, string> userConnections = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 連線事件
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            // 從 request 的 query string 獲取前端的傳來的 userId
            var userId = Context.GetHttpContext().Request.Query["userId"].ToString();

            // 取 JWT token 中的 Claim info
            var name = Context.User.FindFirstValue("name");

            // 關聯連線和 userId
            userConnections[userId] = Context.ConnectionId;

            // 更新聊天內容，通知新連線
            await Clients.All.SendAsync("ReceivePodcast", "新連線 ID "+name+"有嗎", userId);

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 離線事件
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // 更新聊天內容，通知離線
            await Clients.All.SendAsync("ReceivePodcast", "已離線 ID", GetUserIdFromConnectionId(Context.ConnectionId));

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 根據連線 ID 獲取 userId
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        private string GetUserIdFromConnectionId(string connectionId)
        {
            // 使用LINQ查找匹配的鍵
            return userConnections.Where(pair => pair.Value == connectionId)
                        .Select(pair => pair.Key).FirstOrDefault();
        }

        /// <summary>
        /// 根據 userId 獲取連線 ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private string GetConnectionIdFromUserId(string userId)
        {
            if (userConnections.TryGetValue(userId, out var connectionId))
            {
                return connectionId;
            }
            return null;
        }

        public async Task SendMessageToAll(string userId, string message)
        {
            // 將訊息傳送給所有連接的客戶端
            await Clients.All.SendAsync("ReceivePodcast", userId, message);
        }

        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendToGroup(string groupName, string userId, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", userId, message);
        }

        public async Task SendToUser(string sendToId, string userId, string message)
        {
            var toConnectionId = GetConnectionIdFromUserId(sendToId);
            // 接收人
            await Clients.Client(toConnectionId).SendAsync("ReceiveMessage", userId, message);
            // 發送人
            await Clients.Caller.SendAsync("ReceiveMessage", userId, message);
        }
    }
}
