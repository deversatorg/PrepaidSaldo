using ApplicationAuth.Common.Configs;
using ApplicationAuth.Common.Constants.Telegram;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Domain.Entities.Saldo;
using ApplicationAuth.Domain.Entities.Telegram;
using ApplicationAuth.Domain.State;
using ApplicationAuth.Models.Enums;
using ApplicationAuth.Models.RequestModels;
using ApplicationAuth.Models.RequestModels.Saldo;
using ApplicationAuth.Models.RequestModels.Telegram;
using ApplicationAuth.Services.Interfaces;
using ApplicationAuth.Services.Interfaces.Telegram;
using Markdig;
using Microsoft.Bot.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OpenQA.Selenium.DevTools.V111.Fetch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ApplicationAuth.Services.Services.Telegram
{
    public class TelegramCoreService : ITelegramCoreService
    {
        private readonly ITelegramService _telegramService;
        private readonly IAccountService _accountService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IStateFactory _stateFactory;
        private readonly ICacheService _cacheService;
        private readonly ISaldoService _saldoService;
        public TelegramCoreService(IConfiguration configuration,
                                   ITelegramService telegramService,
                                   IAccountService accountService,
                                   IUnitOfWork unitOfWork,
                                   IStateFactory stateFactory,
                                   ICacheService cacheService,
                                   ISaldoService saldoService
                                   )
        {

            
            _telegramService = telegramService;
            _accountService = accountService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _stateFactory = stateFactory;
            _cacheService = cacheService;
            _saldoService = saldoService;
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

        #region Saldo
        public async Task<Message> RegisterSaldo(ITelegramBotClient client, Message message)
        {
            //9690033280
            //8772
            var user = _unitOfWork.Repository<ApplicationUser>().Get(x => x.TelegramId == message.From.Id.ToString())
                                                                .Include(w => w.Saldo)
                                                                .FirstOrDefault();
            if (user.Saldo != null)
                return await SendMessage(client, message.Chat.Id, "Ви вже додали профіль Saldo", replyMarkup: ReplyMarkups.MainMenu());

            // Если регистрация уже в процессе, обрабатываем ответ пользователя
            var userState = _cacheService.Get<IState<SaldoRequestModel>>($"registerSaldo_{message.From.Id}");
            if (userState == null)
            {
                // Новый пользователь, создаем экземпляр UserState и сохраняем его в кэше
                userState = _stateFactory.State<SaldoRequestModel>();
                userState.CurrentStep = 1;
                userState.Id = message.From.Id;
                userState.CountOfSteps = 2;
                userState.Type = Models.Enums.DialogType.SaldoRegistration;

               _cacheService.Set($"registerSaldo_{message.From.Id}", userState, DateTimeOffset.UtcNow.AddMinutes(5));
            }
            else 
            {
                // Обрабатываем ответ в зависимости от текущего состояния диалога
                switch (userState.CurrentStep)
                {
                    case 1:
                        if (message.Text.Length != 10) 
                        {
                            _cacheService.Remove($"registerSaldo_{message.From.Id}");
                            return await client.SendTextMessageAsync(message.Chat.Id, "Невірний формат номера! Код має містити 10 цифр", replyMarkup: ReplyMarkups.MainMenu());
                        }

                        userState.Model.CardNumber = message.Text;

                        userState.CurrentStep = 2; // Переходим к следующему шагу
                        _cacheService.Set($"registerSaldo_{message.From.Id}", userState, DateTimeOffset.UtcNow.AddMinutes(10));
                        return await client.SendTextMessageAsync(message.Chat.Id, "Введіть секретний номер:", replyMarkup: new ForceReplyMarkup());
                    case 2:
                        if (message.Text.Length != 4)     
                        {
                            _cacheService.Remove($"registerSaldo_{message.From.Id}");
                            return await client.SendTextMessageAsync(message.Chat.Id, "Невірний формат кода! Код має містити 4 цифри", replyMarkup: ReplyMarkups.MainMenu());
                        }

                        userState.Model.SecretCode = message.Text;
                        // В этом месте можно сохранить данные в базе данных или выполнять другие операции с полученной информацией
                        user.Saldo = new SaldoProfile() { AccountNumber = userState.Model.CardNumber, SecureCode = userState.Model.SecretCode };
                        _unitOfWork.SaveChanges();
                        // userState.CardNumber и userState.SecretCode содержат введенные данные
                        _cacheService.Remove($"registerSaldo_{message.From.Id}"); // Удаляем запись из кэша
                        return await SendMessage(client, message.Chat.Id, "Успіша регістрація", replyMarkup: ReplyMarkups.MainMenu(true));
                    default:
                        return await SendMessage(client, message.Chat.Id, "Помилка! Спробуйте ще раз", replyMarkup: ReplyMarkups.MainMenu());

                }
            }

            // Запрос номера карты
            return await client.SendTextMessageAsync(message.Chat.Id, "Введіть номер картки:", replyMarkup: new ForceReplyMarkup());

        }

        public async Task<Message> GetBalance(ITelegramBotClient client, Message message)
        {
            var user = _unitOfWork.Repository<ApplicationUser>().Get(x => x.TelegramId == message.From.Id.ToString())
                                                                .Include(w => w.Saldo)
                                                                .FirstOrDefault();
            if (user.Saldo == null)
                return await SendMessage(client, message.Chat.Id, "Не бачимо вашого Saldo. Перевірте чи зареєстрували ви його. Якщо проблема не зникає, то напишіть у підтримку", replyMarkup: ReplyMarkups.InlineMenu());
            var response = await _saldoService.Get(user);

            if (response.Status)
                return await SendMessage(client, message.Chat.Id, $"Ваш Saldo Профіль \n Номер картки: {response.AccountNumber} \n Баланс: {response.Balance} \n Статус картки: Активний", replyMarkup: ReplyMarkups.InlineMenu());

            return await SendMessage(client, message.Chat.Id, $"Ваш Saldo Профіль \n Номер картки: {response.AccountNumber} \n Баланс: {response.Balance} \n Статус картки: Неактивний", replyMarkup: ReplyMarkups.InlineMenu());


        }

        public async Task<Message> DeleteSaldo(ITelegramBotClient client, Message message) 
        {
            var user = _unitOfWork.Repository<ApplicationUser>().Get(x => x.TelegramId == message.From.Id.ToString())
                                                                .Include(w => w.Saldo)
                                                                .FirstOrDefault();
            var cache = _cacheService.GetAllKeyValuePairs();
            foreach (var cacheField in cache)
            {
                if (cacheField.Key.ToString().Contains($"registerSaldo_{message.From.Id}"))
                    _cacheService.Remove(cacheField.Key.ToString());
            }

            if (user.Saldo == null)
                return await SendMessage(client, message.Chat.Id, "Не бачимо вашого Saldo. Перевірте чи зареєстрували ви його. Якщо так і проблема не зникає, то напишіть у підтримку", replyMarkup: ReplyMarkups.InlineMenu());
            var response = await _saldoService.DeleteSaldo(user);
            return await SendMessage(client, message.Chat.Id, response, replyMarkup: ReplyMarkups.MainMenu());

        }

        public async Task<Message> GetTransactionsHistory(ITelegramBotClient client, CallbackQuery callbackQuery, SaldoPaginationRequestModel<SaldoTableColumn> model) 
        {
            var user = _unitOfWork.Repository<ApplicationUser>().Get(x => x.TelegramId == callbackQuery.Message.From.Id.ToString() || x.TelegramId == callbackQuery.Message.Chat.Id.ToString())
                                                                .Include(w => w.Saldo)
                                                                .FirstOrDefault();

            if (user.Saldo == null)
                return await SendMessage(client, callbackQuery.Message.Chat.Id, "Не бачимо вашого Saldo. Перевірте чи зареєстрували ви його. Якщо так і проблема не зникає, то напишіть у підтримку", replyMarkup: ReplyMarkups.InlineMenu());

            var response = await _saldoService.GetTransactionsHistory(model, user);
            var data = new List<string>();

            foreach (var transaction in response.Data) 
            {
                data.Add(transaction.Date + " " + transaction.Company + " " + transaction.Amount);
            }

            //await client.EditMessageTextAsync(message.Chat.Id.ToString(), "Історія за вибраний вами період:", replyMarkup: (InlineKeyboardMarkup)ReplyMarkups.HistoryInlinePagination(data, nameof(GetTransactionsHistory), model.Period, model.CurrentPage, response.TotalCount));
            await client.EditMessageTextAsync(callbackQuery.Message.Chat.Id.ToString(), callbackQuery.Message.MessageId,
                    "Історія за вибраний вами період:",
            replyMarkup: (InlineKeyboardMarkup)ReplyMarkups.HistoryInlinePagination(data, nameof(GetTransactionsHistory), model.Period, model.CurrentPage, response.TotalCount));

            return callbackQuery.Message;
        }
        #endregion

        public async Task<Message> SendInitKeyboard(ITelegramBotClient client, Message message)
        {
            var user = await _accountService.Register(new TelegramUserRegisterRequestModel() { UserId = message.From.Id.ToString() });

            var keyboard = new ReplyKeyboardMarkup
            (
                new List<List<KeyboardButton>>()
                {
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton()
                        {
                            Text = user.Saldo == null?"Реєстрація📝": "Профіль👨‍💼"
                        },
                        new KeyboardButton() {Text = "Баланс💳"},
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton() {Text = "Налаштування⚙️"},
                        new KeyboardButton() {Text = "Підтримка📲"},
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton() {Text = "Автор✍️"},
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

        public async Task<Message> GetProfile(ITelegramBotClient client, Message message)
        {
            
            return await SendMessage(client, message.Chat.Id, "Ви перейшли в меню профілю", replyMarkup: ReplyMarkups.ProfileMenu());
        }

        public string BuildTelegramTable(
            List<string> table_lines,
            string tableColumnSeparator = "|", char inputArraySeparator = ';',
            int maxColumnWidth = 0, bool fixedColumnWidth = false, bool autoColumnWidth = false,
            int minimumColumnWidth = 4, int columnPadRight = 0, int columnPadLeft = 0,
            bool beginEndBorders = true)
        {
            var prereadyTable = new List<string>() { "<pre>" };
            var columnsWidth = new List<int>();
            var firstLine = table_lines[0];
            var lineVector = firstLine.Split(inputArraySeparator);

            if (fixedColumnWidth && maxColumnWidth == 0) throw new ArgumentException("For fixedColumnWidth usage must set maxColumnWidth > 0");
            else if (fixedColumnWidth && maxColumnWidth > 0)
            {
                for (var x = 0; x < lineVector.Length; x++)
                    columnsWidth.Add(maxColumnWidth + columnPadRight + columnPadLeft);
            }
            else
            {
                for (var x = 0; x < lineVector.Length; x++)
                {
                    var columnData = lineVector[x].Trim();
                    var columnFullLength = columnData.Length;

                    if (autoColumnWidth)
                        table_lines.ForEach(line => columnFullLength = line.Split(inputArraySeparator)[x].Length > columnFullLength ? line.Split(inputArraySeparator)[x].Length : columnFullLength);

                    columnFullLength = columnFullLength < minimumColumnWidth ? minimumColumnWidth : columnFullLength;

                    var columnWidth = columnFullLength + columnPadRight + columnPadLeft;

                    if (maxColumnWidth > 0 && columnWidth > maxColumnWidth)
                        columnWidth = maxColumnWidth;

                    columnsWidth.Add(columnWidth);
                }
            }

            foreach (var line in table_lines)
            {
                lineVector = line.Split(inputArraySeparator);

                var fullLine = new string[lineVector.Length + (beginEndBorders ? 2 : 0)];
                if (beginEndBorders) fullLine[0] = "";

                for (var x = 0; x < lineVector.Length; x++)
                {
                    var clearedData = lineVector[x].Trim();
                    var dataLength = clearedData.Length;
                    var columnWidth = columnsWidth[x];
                    var columnSizeWithoutTrimSize = columnWidth - columnPadRight - columnPadLeft;
                    var dataCharsToRead = columnSizeWithoutTrimSize > dataLength ? dataLength : columnSizeWithoutTrimSize;
                    var columnData = clearedData.Substring(0, dataCharsToRead);
                    columnData = columnData.PadRight(columnData.Length + columnPadRight);
                    columnData = columnData.PadLeft(columnData.Length + columnPadLeft);

                    var column = columnData.PadRight(columnWidth);

                    fullLine[x + (beginEndBorders ? 1 : 0)] = column;
                }

                if (beginEndBorders) fullLine[fullLine.Length - 1] = "";

                prereadyTable.Add(string.Join(tableColumnSeparator, fullLine));
            }

            prereadyTable.Add("</pre>");

            return string.Join("\r\n", prereadyTable);
        }

        public async Task<Message> GetHistoryPeriods(ITelegramBotClient client, Message message)
        {
            var user = _unitOfWork.Repository<ApplicationUser>().Get(x => x.TelegramId == message.From.Id.ToString())
                                                                .Include(w => w.Saldo)
                                                                .FirstOrDefault();

            if (user.Saldo == null)
                return await SendMessage(client, message.Chat.Id, "Не бачимо вашого Saldo. Перевірте чи зареєстрували ви його. Якщо так і проблема не зникає, то напишіть у підтримку", replyMarkup: ReplyMarkups.InlineMenu());

            var periods = await _saldoService.GetHistoryPeriods(user);

            return await SendMessage(client, message.Chat.Id, "Виберіть за який період хочете переглянути исторію:", replyMarkup: ReplyMarkups.PeriodsInlinePagination(periods, nameof(GetTransactionsHistory)));
        }

        public async Task<Message> GetTransaction(ITelegramBotClient client, CallbackQuery callbackQuery, string transaction, int page, string period)
        {
            var user = _unitOfWork.Repository<ApplicationUser>().Get(x => x.TelegramId == callbackQuery.Message.Chat.Id.ToString())
                                                               .Include(w => w.Saldo)
                                                               .FirstOrDefault();

            if (user.Saldo == null)
                return await SendMessage(client, callbackQuery.Message.Chat.Id, "Не бачимо вашого Saldo. Перевірте чи зареєстрували ви його. Якщо так і проблема не зникає, то напишіть у підтримку", replyMarkup: ReplyMarkups.InlineMenu());


            var response = await _saldoService.GetTransaction(user, transaction, page, period);

           return await client.SendTextMessageAsync(callbackQuery.Message.Chat.Id.ToString(),
                "Дата та час : " + response.Date + "\n" +
                "Компанія : " + response.Company + "\n" +
                "Карта/Кеш : " + response.DebitCredit + "\n" +
                "Тип транзакції : " + response.TransactionType + "\n" +
                "Сума : " + response.Amount + "\n" +
                "Опис транзакції: " + response.Description + "\n"
                );

        }
    }
}
