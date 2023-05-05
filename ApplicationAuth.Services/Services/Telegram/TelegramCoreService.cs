using ApplicationAuth.Common.Configs;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Domain.Entities.Saldo;
using ApplicationAuth.Models.RequestModels.Telegram;
using ApplicationAuth.Services.Interfaces;
using ApplicationAuth.Services.Interfaces.Telegram;
using Microsoft.Bot.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ApplicationAuth.Services.Services.Telegram
{
    public class TelegramCoreService : ITelegramCoreService
    {
        private readonly ITelegramService _telegramService;
        private readonly IAccountService _accountService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public TelegramCoreService(IConfiguration configuration,
                                   ITelegramService telegramService,
                                   IAccountService accountService,
                                   IUnitOfWork unitOfWork
                                   )
        {

            
            _telegramService = telegramService;
            _accountService = accountService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<Message> SendMessage(ITelegramBotClient client ,long chatId, string text, IReplyMarkup? replyMarkup = null)
        {
            try
            {
                var response = await client.SendTextMessageAsync(chatId, text, replyMarkup: replyMarkup);
                return response;
            }
            catch (Exception ex)
            {
                throw new ApplicationException();
            }
        }


        public async Task<Message> RegisterSaldo(ITelegramBotClient client,Message message) 
        {
            //9690033280
            //8772
            var user = _unitOfWork.Repository<ApplicationUser>().Get(x => x.TelegramId == message.From.Id.ToString())
                                                                .Include(w => w.Saldo)
                                                                .FirstOrDefault();
            if (user.Saldo != null)
                return await SendMessage(client, message.Chat.Id, "Ви вже додали профіль Saldo", replyMarkup: _mainMenu());

            var saldo = new SaldoProfile();

            var response = await client.SendTextMessageAsync(message.Chat.Id, "Введіть номер картки:", replyMarkup: new ForceReplyMarkup());
            saldo.AccountNumber = await PollForUserInput(client, message, response, "Невірний формат номера! ", 10);

            response = await client.SendTextMessageAsync(message.Chat.Id, "Введіть секретний номер:", replyMarkup: new ForceReplyMarkup());
            saldo.SecureCode = await PollForUserInput(client, message, response, "Невірний формат кода!", 4);

            if (saldo.AccountNumber == null || saldo.SecureCode == null)
                return await SendMessage(client, message.Chat.Id, "Помилка! Перевірте правильність написанних вами даних", _mainMenu());

            _unitOfWork.SaveChanges();

            return await SendMessage(client, message.Chat.Id, "Успіша регістрація", _mainMenu());
        }

        public async Task<Message> GetBalance(ITelegramBotClient client, Message message)
        {
            var response = await _telegramService.GetSaldo(message.From.Id.ToString());

            if (response.Status)
                return await SendMessage(client, message.Chat.Id, $"Ваш Saldo Профіль \n Номер картки: {response.AccountNumber} \n Баланс: {response.Balance} \n Статус картки: Активний", replyMarkup: _inlineMenu());

            return await SendMessage(client, message.Chat.Id, $"Ваш Saldo Профіль \n Номер картки: {response.AccountNumber} \n Баланс: {response.Balance} \n Статус картки: Неактивний", replyMarkup: _inlineMenu());


        }

        public async Task<Message> SendInitKeyboard(ITelegramBotClient client, Message message)
        {
            await _accountService.Register(new TelegramUserRegisterRequestModel() { UserId = message.From.Id.ToString() });

            var keyboard = new ReplyKeyboardMarkup
            (
                new List<List<KeyboardButton>>()
                {
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton() {Text = "Реєстрація📝"},
                        new KeyboardButton() {Text = "Баланс💳"},
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton() {Text = "/start"},
                    }
                }
            )
            {
                ResizeKeyboard = true
            };

            return await client.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: "Виберіть щось з меню",
                                                      replyMarkup: keyboard);

        }

        #region Menus
        private IReplyMarkup _inlineMenu()
        {
            var keyboard = new InlineKeyboardMarkup
            (
                new List<InlineKeyboardButton>()
                {
                    new InlineKeyboardButton() {Text = "Головне меню", CallbackData= "/start"},
                }
            );
            
            return keyboard;
        }

        private IReplyMarkup _mainMenu()
        {
            var keyboard = new ReplyKeyboardMarkup
            (
                new List<List<KeyboardButton>>()
                {
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton() {Text = "Реєстрація📝"},
                        new KeyboardButton() {Text = "Баланс💳"},
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton() {Text = "/start"},
                    }
                }
            )
            {
                ResizeKeyboard = true
            };

            return keyboard;
        }
        #endregion

        private async Task<string> PollForUserInput(ITelegramBotClient client, Message message, Message response, string errorMessage, int expectedLength)
        {
            var hook = @$"{_configuration.GetSection("BotConfiguration").Get<BotConfiguration>().HostAddress}/api/bot/update";
            await client.DeleteWebhookAsync(true);

            Update[] updates;
            while (true)
            {
                updates = await client.GetUpdatesAsync(
                    offset: response.MessageId + 1,
                    limit: 10
                );

                foreach (var update in updates)
                {
                    if (update.Message.From.Id == message.From.Id &&
                        update.Message.Chat.Id == message.Chat.Id &&
                        update.Message.ReplyToMessage?.MessageId == response.MessageId &&
                        update.Message.Text != null &&
                        update.Message.Text.Length == expectedLength)
                    {
                        await client.SetWebhookAsync(hook);
                        return update.Message.Text;
                    }
                }

                await Task.Delay(1000); // Wait for 1 second before checking for updates again
            }
        }
    }
}
