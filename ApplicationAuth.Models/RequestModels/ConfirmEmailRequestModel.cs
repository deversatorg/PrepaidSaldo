using System.ComponentModel.DataAnnotations;

namespace ApplicationAuth.Models.RequestModels
{
    public class ConfirmEmailRequestModel
    {
        [Required(ErrorMessage = "Token field is empty")]
        public string Token { get; set; }
    }

}
