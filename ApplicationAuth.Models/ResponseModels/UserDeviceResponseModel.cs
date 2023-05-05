using Newtonsoft.Json;
using System;

namespace ApplicationAuth.Models.ResponseModels
{
    public class UserDeviceResponseModel
    {
        [JsonRequired]
        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonRequired]
        [JsonProperty("isVerified")]
        public bool IsVerified { get; set; }

        [JsonRequired]
        [JsonProperty("deviceToken")]
        public string DeviceToken { get; set; }

        [JsonRequired]
        [JsonProperty("addedAt")]
        public DateTime AddedAt { get; set; }
    }
}
