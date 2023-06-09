using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ApplicationAuth.Models.RequestModels
{
    public class AdminLoginRequestModel : EmailRequestModel
    {
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [JsonProperty("accessTokenLifetime")]
        public int? AccessTokenLifetime { get; set; }
    }
}
