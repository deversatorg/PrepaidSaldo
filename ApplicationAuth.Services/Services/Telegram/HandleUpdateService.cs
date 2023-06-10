﻿using Microsoft.Extensions.Logging;
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
                    int? page = int.Parse(parameters["page"]);
                    string? period = parameters["period"];

                    var response = await _coreService.GetTransaction(_botClient, callbackQuery, transaction, page==null?1:page.Value, period);
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
