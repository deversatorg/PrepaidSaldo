using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Models.ResponseModels.Telegram
{
    public class TelegramMessageResponseModel
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public string SenderId { get; set; }

        public string SentAt { get; set; }
    }
}
