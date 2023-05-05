using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ApplicationAuth.Services.Interfaces.Telegram
{
    public interface ITelegramCoreService
    {
        Task<Message> SendInitKeyboard(ITelegramBotClient client, Message message);
        Task<Message> RegisterSaldo(ITelegramBotClient client, Message message);
        Task<Message> GetBalance(ITelegramBotClient client, Message message);
    }
}
