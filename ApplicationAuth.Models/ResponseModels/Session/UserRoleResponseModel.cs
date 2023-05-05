using Newtonsoft.Json;

namespace ApplicationAuth.Models.ResponseModels.Session
{
    public class UserRoleResponseModel : UserResponseModel
    {
        [JsonProperty("role")]
        public string Role { get; set; }
    }
}
