using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ApplicationAuth.Common.Constants.Telegram
{
    public static class ReplyMarkups
    {
        public static IReplyMarkup InlineMenu()
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

        public static IReplyMarkup MainMenu(bool isRegistered = false)
        {
            var keyboard = new ReplyKeyboardMarkup
             (
                 new List<List<KeyboardButton>>()
                 {
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton()
                        {
                            Text = !isRegistered?"Реєстрація📝": "Профіль👨‍💼"
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


            return keyboard;
        }

        public static IReplyMarkup YesOrNo(string positiveResponseCallBack, string negativeResponseCallback)
        {
            var keyboard = new InlineKeyboardMarkup
            (
                 new List<InlineKeyboardButton>()
                 {
                    new InlineKeyboardButton() {Text = "Так", CallbackData=positiveResponseCallBack},
                    new InlineKeyboardButton() {Text = "Ні", CallbackData=negativeResponseCallback},
                 }
             );
            return keyboard;
        }

        public static IReplyMarkup NotImplimented()
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

        public static IReplyMarkup ProfileMenu() 
        {
            var keyboard = new ReplyKeyboardMarkup
            (
                new List<List<KeyboardButton>>()
                {
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton() {Text = "Виписка📃"},
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton() {Text = "Видалити Saldo🗑"},
                        new KeyboardButton() {Text = "Баланс💳"},
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton() {Text = "Назад"},
                    }
                }
            )
            {
                ResizeKeyboard = true
            };

            return keyboard;
        }
    }
}
