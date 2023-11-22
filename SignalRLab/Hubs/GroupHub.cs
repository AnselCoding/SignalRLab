using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;
using SignalRSwaggerGen.Enums;
using System.Security.Claims;

namespace SignalRLab.Hubs
{

    [SignalRHub(autoDiscover: AutoDiscover.MethodsAndParams, documentNames: new[] { "hubs" })] //顯示於 Swagger 指定分頁中
    [Authorize(Policy = "PolicyForPath2")]
    public class GroupHub : BaseHub
    {
        private static Dictionary<string, List<string>> _userGroups = new Dictionary<string, List<string>>();

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
                // 從组中移除用户
                foreach (var group in _userGroups[sendFromId])
                {
                    await Clients.OthersInGroup(group).SendAsync("ReceiveGroupMessage", $"{sendFromId}", "已離線");
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
                    
                }
                _userGroups.Remove(sendFromId);

            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task AddToGroup(string groupName)
        {
            var sendFromId = Context.User.FindFirstValue("name");
            _userGroups[sendFromId] = new List<string>() { groupName };
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendToGroup(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", Context.User.FindFirstValue("name"), message);
        }
    }
}
