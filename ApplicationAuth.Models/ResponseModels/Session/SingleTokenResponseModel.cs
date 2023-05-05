using Newtonsoft.Json;

namespace ApplicationAuth.Models.ResponseModels.Session
{
    public class SingleTokenResponseModel
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
