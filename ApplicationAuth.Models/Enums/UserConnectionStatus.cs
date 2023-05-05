using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ApplicationAuth.Models.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserConnectionStatus
    {
        Offline,
        Online
    }
}
