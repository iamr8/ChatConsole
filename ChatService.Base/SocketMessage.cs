using Newtonsoft.Json;

namespace ChatService.Base;

public record SocketMessage
{
    [JsonProperty("u")]
    public string User { get; set; }
    [JsonProperty("m")]
    public string Message { get; set; }
    [JsonProperty("i")]
    public bool Internal { get; set; }
}