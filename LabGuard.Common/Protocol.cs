namespace LabGuard.Common
{
    public static class Protocol
    {
        // Change this before deployment
        public const string PSK = "CHANGE_ME_PSK_12345";
    }

    public enum ClientStatus
    {
        Normal,
        Misuse,
        Offline
    }

    public record ClientInfo(string Id, string Hostname, ClientStatus Status, string? Details = null);
}
