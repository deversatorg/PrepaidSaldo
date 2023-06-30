using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using ApplicationAuth.Services.Interfaces.Telegram;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using ApplicationAuth.Services.Interfaces;
using ApplicationAuth.Domain.State;
using ApplicationAuth.Models.RequestModels.Saldo;
using ApplicationAuth.Models.RequestModels;
using ApplicationAuth.Models.Enums;
using Microsoft.Bot.Schema.Teams;
using ApplicationAuth.Common.Constants.Telegram;

namespace ApplicationAuth.Services.Services.Telegram
{
    public class HandleUpdateService : IHandleUpdateService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ITelegramCoreService _coreService;
        private readonly ILogger<HandleUpdateService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;

        public HandleUpdateService(ITelegramBotClient botClient, 
                                   ILogger<HandleUpdateService> logger,
                                   IUnitOfWork unitOfWork,
                                   ITelegramCoreService coreService,
                                   ICacheService cacheService)
        {
            _botClient = botClient;
            _logger = logger;
            _coreService = coreService;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task EchoAsync(Update update)
        {
            var handler = update.Type switch
            {
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                UpdateType.Message => BotOnMessageReceived(update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage!),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery!),
                UpdateType.InlineQuery => BotOnInlineQueryReceived(update.InlineQuery!),
                UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult!),
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(exception);
            }
        }

        private async Task BotOnMessageReceived(Message message)
        {
            _logger.LogInformation("Receive message type: {messageType}", message.Type);
            if (message.Type != MessageType.Text)
                return;

            var action = message.Text!.Split(' ')[0] switch
            {
                "Баланс💳" => _coreService.GetBalance(_botClient, message), 
                "Реєстрація📝" => _coreService.RegisterSaldo(_botClient, message),
                "Профіль👨‍💼" => _coreService.GetProfile(_botClient, message),
                "Виписка📃" => _coreService.GetHistoryPeriods(_botClient, message),
                "Видалити" => _coreService.DeleteSaldo(_botClient, message),
                "Автор✍️" => Author(_botClient, message),
                "Підтримка📲" => Support(_botClient, message),
                "Назад" => _coreService.SendInitKeyboard(_botClient, message),
                "/start" => _coreService.SendInitKeyboard(_botClient, message),
                "/inline" => SendInlineKeyboard(_botClient, message),
                "/keyboard" => SendReplyKeyboard(_botClient, message),
                "/remove" => RemoveKeyboard(_botClient, message),
                "/photo" => SendFile(_botClient, message),
                "/request" => RequestContactAndLocation(_botClient, message),
                _ => StateCheck(_botClient, message)
                //TODO: IF unknown command - check states in cache and transist to method;
            };
            Message sentMessage = await action;
            _logger.LogInformation("The message was sent with id: {sentMessageId}", sentMessage.MessageId);

            // Send inline keyboard
            // You can process responses in BotOnCallbackQueryReceived handler
            static async Task<Message> SendInlineKeyboard(ITelegramBotClient bot, Message message)
            {
                await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                // Simulate longer running task
                await Task.Delay(500);

                InlineKeyboardMarkup inlineKeyboard = new(
                    new[]
                    {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1.1", "11"),
                        InlineKeyboardButton.WithCallbackData("1.2", "12"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22"),
                    },
                    });

                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: "Choose",
                                                      replyMarkup: inlineKeyboard);
            }

            static async Task<Message> SendReplyKeyboard(ITelegramBotClient bot, Message message)
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(
                    new[]
                    {
                        new KeyboardButton[] { "1.1", "1.2" },
                        new KeyboardButton[] { "2.1", "2.2" },
                    })
                {
                    ResizeKeyboard = true
                };

                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: "Choose",
                                                      replyMarkup: replyKeyboardMarkup);
            }

            static async Task<Message> RemoveKeyboard(ITelegramBotClient bot, Message message)
            {
                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: "Removing keyboard",
                                                      replyMarkup: new ReplyKeyboardRemove());
            }

            static async Task<Message> SendFile(ITelegramBotClient bot, Message message)
            {
                await bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                const string filePath = @"Files/tux.png";
                using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

                return await bot.SendPhotoAsync(chatId: message.Chat.Id,
                                                photo: new InputOnlineFile(fileStream, fileName),
                                                caption: "Nice Picture");
            }

            static async Task<Message> RequestContactAndLocation(ITelegramBotClient bot, Message message)
            {
                ReplyKeyboardMarkup RequestReplyKeyboard = new(
                    new[]
                    {
                    KeyboardButton.WithRequestLocation("Location"),
                    KeyboardButton.WithRequestContact("Contact"),
                    });

                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: "Who or Where are you?",
                                                      replyMarkup: RequestReplyKeyboard);
            }

            static async Task<Message> Usage(ITelegramBotClient bot, Message message)
            {
                const string usage = "Usage:\n" +
                                     "/inline   - send inline keyboard\n" +
                                     "/keyboard - send custom keyboard\n" +
                                     "/remove   - remove custom keyboard\n" +
                                     "/photo    - send a photo\n" +
                                     "/request  - request location or contact";

                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: usage,
                                                      replyMarkup: new ReplyKeyboardRemove());
            }

            async Task<Message> StateCheck(ITelegramBotClient bot, Message message) 
            {
                var cacheFields = _cacheService.GetAllKeyValuePairs();
                Dictionary<string, Action> actions = new Dictionary<string, Action>();
                actions["registerSaldo"] = () => _coreService.RegisterSaldo(bot, message);
                actions["example"] = () => Console.WriteLine("Found 'example' in the input");
                foreach (var action in actions)
                {
                    foreach (var cacheField in cacheFields)
                    {
                        if (cacheField.Key.ToString().Contains($"{action.Key}_{message.From.Id}")) 
                        {
                            try
                            {
                                action.Value();
                            }
                            catch (Exception ex)
                            {
                                await HandleErrorAsync(ex);
                            }
                        }
                    }
                }
                return new Message();
                
            }

            static async Task<Message> Author(ITelegramBotClient bot, Message message) 
            {
                const string text = "Привіт! Я автор цього Telegram-бота.\n\n" +
                    "Я займаюся розробкою програмного забезпечення і створив цього бота для більш зручного перегляду даних з веб-сайту (https://www.prepaidsaldo.com) та надання інформації про стан балансу та історію транзакцій.\n\n" +
                    "Якщо у вас є будь-які питання, пропозиції або відгуки, не соромтеся зв'язатися зі мною.\n\n" +
                    "Також, якщо вам сподобався мій бот і ви хочете підтримати мою роботу:\n\n" +
                    "0xEE9e7123087e3A83E1DC96699506aFA04BddF3F5 - USDT\n\n" +
                    "4569 3320 0289 2125 - UAH \n\n" +
                    "Дякую, що використовуєте мого бота, сподіваюся, він буде корисний для вас! 😃👍";

                return await bot.SendTextMessageAsync(chatId: message.Chat.Id, text: text, disableWebPagePreview: true);
            }

            static async Task<Message> Support(ITelegramBotClient bot, Message message)
            {
                const string text = "🆘 Потрібна підтримка? Не хвилюйтесь, я тут, щоб допомогти!\n\n" +
                    "🤝 Якщо у вас виникли проблеми з моїм ботом, я з радістю допоможу їх вирішити. " +
                    "@deversatorg - мій профіль в Телеграм.\n\n" +
                    "⚙️ Я стежу за роботою свого бота і намагаюся забезпечити його найкращу продуктивність. " +
                    "Але якщо ви виявите будь-які неполадки або маєте пропозиції щодо покращення, будь ласка, " +
                    "повідомте мене і я зроблю все можливе, щоб ви були задоволені.\n\n" +
                    "🙏 Дякую вам за використання мого бота і довіру! Ваша зворотна зв'язок та підтримка важливі для мене " +
                    "і допомагають покращити роботу бота.\n\n" +
                    "Завжди радий допомогти вам! 🤗";

                return await bot.SendTextMessageAsync(chatId: message.Chat.Id, text: text, disableWebPagePreview: true);
            }

        }

        // Process Inline Keyboard callback data
        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            await _botClient.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.Typing);
            Func<Task<Message>> action = callbackQuery.Data!.Split('?')[0] switch
            {
                "/" + nameof(_coreService.GetTransactionsHistory) => async () =>
                {
                    var parameters = callbackQuery.Data.Split('?')[1]
                        .Split('&')
                        .Select(param => param.Split('='))
                        .ToDictionary(parts => parts[0], parts => parts[1]);

                    var page = int.Parse(parameters["page"]);
                    var period = parameters["period"];

                    var transactionsHistory = await _coreService.GetTransactionsHistory(_botClient,callbackQuery,new SaldoPaginationRequestModel<SaldoTableColumn>() { CurrentPage = page, Limit = 6, Period = period});
                    
                    return transactionsHistory;
                },

                "/T" => async () => 
                {
                    var parameters = callbackQuery.Data.Split('?')[1]
                        .Split('&')
                        .Select(param => param.Split('='))
                        .ToDictionary(parts => parts[0], parts => parts[1]);

                    var transaction = parameters["t"];
                    int page = int.Parse(parameters["page"]);
                    string period = parameters["period"];

                    var response = await _coreService.GetTransaction(_botClient, callbackQuery, transaction, page, period);
                    return response;
                }
                ,
                _ => new Func<Task<Message>>(async () => { return new Message(); }),
                //TODO: IF unknown command - check states in cache and transist to method;
            };
            Message nessage = await action.Invoke();

            /*await _botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Received {callbackQuery.Data}");*/

        }

        #region Inline Mode

        private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery)
        {
            _logger.LogInformation("Received inline query from: {inlineQueryFromId}", inlineQuery.From.Id);

            InlineQueryResultBase[] results =
            {
                // displayed result
                new InlineQueryResultArticle
                (
                    id: "3",
                    title: "TgBots",
                    inputMessageContent: new InputTextMessageContent
                    (
                        "hello"
                    )
                )
            };

            await _botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id,
                                                    results: results,
                                                    isPersonal: true,
                                                    cacheTime: 0);
        }

        private Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult)
        {
            _logger.LogInformation("Received inline result: {chosenInlineResultId}", chosenInlineResult.ResultId);
            return Task.CompletedTask;
        }

        #endregion

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            _logger.LogInformation("Unknown update type: {updateType}", update.Type);
            return Task.CompletedTask;
        }

        public Task HandleErrorAsync(Exception exception)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError("HandleError: {ErrorMessage}", ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
