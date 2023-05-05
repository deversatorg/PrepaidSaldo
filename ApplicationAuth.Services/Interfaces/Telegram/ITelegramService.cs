using ApplicationAuth.Models.RequestModels.Telegram;
using ApplicationAuth.Models.ResponseModels.Saldo;
using ApplicationAuth.Models.ResponseModels.Telegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ApplicationAuth.Services.Interfaces.Telegram
{
    public interface ITelegramService
    {
        public Task<SaldoResponseModel> GetSaldo(string telegramId);
    }
}
