using Newtonsoft.Json;

namespace ApplicationAuth.Models.ResponseModels.Session
{
    public class TokenResponseModel
    {
        [JsonRequired]
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonRequired]
        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }

        [JsonRequired]
        [JsonProperty("expireDate")]
        public string ExpireDate { get; set; }

        [JsonRequired]
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
