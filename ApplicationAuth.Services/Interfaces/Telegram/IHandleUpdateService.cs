using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ApplicationAuth.Services.Interfaces.Telegram
{
    public interface IHandleUpdateService
    {
        Task EchoAsync(Update update);
    }
}
