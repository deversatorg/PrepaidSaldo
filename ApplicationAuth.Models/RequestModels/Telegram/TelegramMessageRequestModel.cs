using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ApplicationAuth.Models.RequestModels.Telegram
{
    public class TelegramMessageRequestModel
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [Required(ErrorMessage = "userToken field is empty")]
        [JsonProperty("userToken")]
        public string UserToken { get; set; }
    }
}
