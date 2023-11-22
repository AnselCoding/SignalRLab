using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;
using SignalRSwaggerGen.Enums;
using System.Security.Claims;

namespace SignalRLab.Hubs
{

    [SignalRHub(autoDiscover: AutoDiscover.MethodsAndParams, documentNames: new[] { "hubs" })] //顯示於 Swagger 指定分頁中
    [Authorize(Policy = "PolicyForPath2")]
    public class PersonHub : BaseHub
    {
        /// <summary>
        /// 離線事件
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        [SignalRHidden]
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // 更新聊天內容，通知離線
            // 獲取使用者 ID
            var sendFromId = Context.User.FindFirstValue("name");

            if (!string.IsNullOrEmpty(sendFromId))
            {
                // 更新聊天內容，通知離線
                await Clients.OthersInGroup(sendFromId).SendAsync("ReceiveMessage", $"{sendFromId}", "已離線");

                // 從组中移除用户
                // 無法取得群組中的ConnectionId
                //await Groups.RemoveFromGroupAsync(toConnectionId, sendFromId);
            }

            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendToUser(string sendToId, string message)
        {
            var toConnectionId = GetConnectionIdFromUserId(sendToId);

            // 將離線時需要通知的人，加入群組
            await Groups.AddToGroupAsync(Context.ConnectionId, sendToId);
            await Groups.AddToGroupAsync(toConnectionId, Context.User.FindFirstValue("name"));


            // 接收人
            await Clients.Client(toConnectionId).SendAsync("ReceiveMessage", Context.User.FindFirstValue("name"), message);
            // 發送人
            await Clients.Caller.SendAsync("ReceiveMessage", Context.User.FindFirstValue("name"), message);
        }
    }
}
