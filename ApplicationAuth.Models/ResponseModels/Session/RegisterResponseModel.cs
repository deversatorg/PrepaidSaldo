using Newtonsoft.Json;

namespace ApplicationAuth.Models.ResponseModels.Session
{
    public class RegisterResponseModel
    {
        [JsonRequired]
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}