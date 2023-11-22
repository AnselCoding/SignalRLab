using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;
using SignalRSwaggerGen.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;

namespace SignalRLab.Hubs
{
    public class BaseHub : Hub
    {
        // 存放 JWT token userId 與 ConnectionId 的配對
        protected static readonly ConcurrentDictionary<string, string> userConnectionIds = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 連線事件
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            // 取 JWT token 中的 Claim info
            var userId = Context.User.FindFirstValue("name");

            // 關聯連線和 userId
            userConnectionIds[userId] = Context.ConnectionId;

            // 每個使用者都開一個群組，存入所有的登入裝置連線 (一個使用者可能同時登入多個裝置)
            //await Groups.AddToGroupAsync(Context.ConnectionId, userId);

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 根據連線 ID 獲取 userId
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        protected string GetUserIdFromConnectionId(string connectionId)
        {
            // 使用LINQ查找匹配的鍵
            return userConnectionIds.Where(pair => pair.Value == connectionId)
                        .Select(pair => pair.Key).FirstOrDefault();
        }

        /// <summary>
        /// 根據 userId 獲取連線 ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        protected string GetConnectionIdFromUserId(string userId)
        {
            if (userConnectionIds.TryGetValue(userId, out var connectionId))
            {
                return connectionId;
            }
            return null;
        }       

    }
}
