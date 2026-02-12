using System;

namespace LabGuard.Common
{
    public enum MessageType { StatusUpdate, Command, Acknowledgement }
    public enum CommandType { Warn, Screenshot, Shutdown }

    public class BaseMessage
    {
        public MessageType MessageType { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Psk { get; set; }
    }

    public class StatusUpdateMessage : BaseMessage
    {
        public ClientStatus Status { get; set; }
        public string? Details { get; set; }
    }

    public class CommandMessage : BaseMessage
    {
        public CommandType Command { get; set; }
        public string? Payload { get; set; }
    }

    public static class MessageSerializer
    {
        private static readonly System.Text.Json.JsonSerializerOptions _opts = new()
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        public static byte[] Serialize(BaseMessage msg)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(msg, msg.GetType(), _opts);
            var payload = System.Text.Encoding.UTF8.GetBytes(json);
            var len = BitConverter.GetBytes(payload.Length);
            if (!BitConverter.IsLittleEndian) Array.Reverse(len);
            var outp = new byte[4 + payload.Length];
            Array.Copy(len, 0, outp, 0, 4);
            Array.Copy(payload, 0, outp, 4, payload.Length);
            return outp;
        }

        public static BaseMessage? DeserializeFromSpan(ReadOnlySpan<byte> span)
        {
            var json = System.Text.Encoding.UTF8.GetString(span);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("messageType", out var mt)) return null;
            var mtv = mt.GetString();
            Type t = typeof(BaseMessage);
            if (mtv == "StatusUpdate") t = typeof(StatusUpdateMessage);
            else if (mtv == "Command") t = typeof(CommandMessage);
            return (BaseMessage?)System.Text.Json.JsonSerializer.Deserialize(json, t, _opts);
        }
    }
}
