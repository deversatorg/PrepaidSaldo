using ApplicationAuth.Models.Enums;
using ApplicationAuth.Models.RequestModels;
using ApplicationAuth.Models.RequestModels.Saldo;
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
        Task<Message> GetProfile(ITelegramBotClient client, Message message);

        Task<Message> RegisterSaldo(ITelegramBotClient client, Message message);
        Task<Message> GetBalance(ITelegramBotClient client, Message message);
        Task<Message> DeleteSaldo(ITelegramBotClient client, Message message);

        Task<Message> GetTransactionsHistory(ITelegramBotClient client, CallbackQuery callbackQuery, SaldoPaginationRequestModel<SaldoTableColumn> model);
        Task<Message> GetTransaction(ITelegramBotClient client, CallbackQuery callbackQuery, string transaction, int page, string period);
        Task<Message> GetHistoryPeriods(ITelegramBotClient client, Message message);
    }
}
