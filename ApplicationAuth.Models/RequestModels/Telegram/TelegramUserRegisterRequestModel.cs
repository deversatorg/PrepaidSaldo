using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Models.RequestModels.Telegram
{
    public class TelegramUserRegisterRequestModel
    {
        public string UserId { get; set; }
        public string? UserName { get; set; }
    }
}
