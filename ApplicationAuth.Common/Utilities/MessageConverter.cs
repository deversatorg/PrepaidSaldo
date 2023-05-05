using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ApplicationAuth.Common.Utilities
{
    public class MessageConverter : Newtonsoft.Json.JsonConverter<Message>
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override Message ReadJson(JsonReader reader, Type objectType, Message existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            // Создаем новый объект Message
            var message = new Message();

            // Загружаем JSON-объект из потока чтения
            var jsonObject = JObject.Load(reader);

            // Десериализуем поля объекта Message из JSON-объекта
            message.MessageId = jsonObject["message_id"].Value<int>();
            message.Text = jsonObject["text"].Value<string>();
            message.Chat.Id = jsonObject["chat"]["id"].Value<long>();

            // Возвращаем десериализованный объект Message
            return message;
        }

        public override void WriteJson(JsonWriter writer, Message value, Newtonsoft.Json.JsonSerializer serializer)
        {
            // Метод WriteJson не реализуется, так как конвертер только для чтения
            throw new NotImplementedException();
        }
    }
}
