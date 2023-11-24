using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;
using SignalRSwaggerGen.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;

namespace SignalRLab.Hubs
{
    public class BaseHub : Hub<IBaseHub>
    {
        // 存放 JWT token userId 與 ConnectionId 的配對
        protected static readonly ConcurrentDictionary<string, string> userConnectionIds = new ConcurrentDictionary<string, string>();

        protected string UserId 
        { 
            get { return Context.User.FindFirstValue("name"); } 
        }

        //public BaseHub() { }

        /// <summary>
        /// 連線事件
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            // 取 JWT token 中的 Claim info
            var userId = UserId;

            // 關聯連線和 userId
            //userConnectionIds[userId] = Context.ConnectionId;
            // TryAdd 不存在字典中就加入，已經存在就不加入
            userConnectionIds.TryAdd(userId, Context.ConnectionId);

            // 手動建立:每個使用者都開一個群組，存入所有的登入裝置連線 (一個使用者可能同時登入多個裝置)
            // 可以使用JWT Token確認身分，自動辨識
            //await Groups.AddToGroupAsync(Context.ConnectionId, userId);

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
            // 取 JWT token 中的 Claim info
            var userId = UserId;

            // 移除關聯連線和 userId
            userConnectionIds.TryRemove(userId, out var connectionId);

            await base.OnDisconnectedAsync(exception);
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
