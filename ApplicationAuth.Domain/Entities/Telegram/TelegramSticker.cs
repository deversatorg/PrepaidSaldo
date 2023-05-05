using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Domain.Entities.Telegram
{
    public class TelegramSticker : IEntity
    {
        public int Id { get; set; }

        public string StickerId { get; set; }

        public string FileUniqueId { get; set; }

        public DateTime DateTime { get; set; }

        public int CountOfUsage { get; set; }

    }
}
