using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SignalRLab.Middleware
{
    public class MyUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // 自訂使用者識別邏輯
            return connection.User?.FindFirstValue("name");
        }
    }
}
