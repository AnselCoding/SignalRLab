using Microsoft.AspNetCore.SignalR;

namespace SignalRLab.SignalRTimer
{
    public interface IDisconnectTimer<T>
    {
        void Resrart();
        Task Start();
        void Stop();
    }
}