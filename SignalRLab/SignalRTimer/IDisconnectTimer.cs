namespace SignalRLab.SignalRTimer
{
    public interface IDisconnectTimer
    {
        void Restart();
        void Start();
        void Stop();
    }
}