using SignalRSwaggerGen.Attributes;
using SignalRSwaggerGen.Enums;

namespace SignalRLab.Hubs
{
    [SignalRHub(autoDiscover: AutoDiscover.MethodsAndParams, documentNames: new[] { "hubs" })] //顯示於 Swagger 指定分頁中
    public interface IBaseHub
    {
        [SignalRMethod(summary: "接收廣播事件", autoDiscover: AutoDiscover.Params)]
        Task OnReceivePodcast(string arg1, string arg2);

        [SignalRMethod(summary: "接收群組訊息事件", autoDiscover: AutoDiscover.Params)]
        Task OnReceiveGroupMessage(string arg1, string arg2);

        [SignalRMethod(summary: "接收個人帳號訊息事件", autoDiscover: AutoDiscover.Params)]
        Task OnReceiveMessage(string arg1, string arg2);

        [SignalRMethod(summary: "接聽Server中斷連線事件", autoDiscover: AutoDiscover.Params)]
        Task OnDisconnect();
    }
}
