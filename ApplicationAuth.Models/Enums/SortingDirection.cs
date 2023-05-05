using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ApplicationAuth.Models.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SortingDirection
    {
        Asc,
        Desc
    }
}
