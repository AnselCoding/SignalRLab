namespace SignalRLab.Models
{
    public record ConnectMemberPairCommand(
    string Action,
    string FromMemberId,
    string ToMemberId,
    string Timestamp);
}
