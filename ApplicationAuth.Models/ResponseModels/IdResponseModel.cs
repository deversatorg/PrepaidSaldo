using Newtonsoft.Json;

namespace ApplicationAuth.Models.ResponseModels
{
    public class IdResponseModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
