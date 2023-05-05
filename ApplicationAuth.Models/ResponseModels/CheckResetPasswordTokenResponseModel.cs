using Newtonsoft.Json;

namespace ApplicationAuth.Models.ResponseModels
{
    public class CheckResetPasswordTokenResponseModel
    {
        [JsonRequired]
        [JsonProperty("isValid")]
        public bool IsValid { get; set; }
    }
}