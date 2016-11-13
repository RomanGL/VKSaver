using IF.Lastfm.Core.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace VKSaver.Core.Services.Json
{
    public sealed class LastImageSetConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LastImageSet);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.ReadFrom(reader);
            return LastImageSet.ParseJToken(token);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var images = (LastImageSet)value;
            writer.WriteStartArray();

            TryWrite("small", images.Small, writer);
            TryWrite("medium", images.Medium, writer);
            TryWrite("large", images.Large, writer);
            TryWrite("extralarge", images.ExtraLarge, writer);
            TryWrite("mega", images.Mega, writer);

            writer.WriteEndArray();
        }

        private static void TryWrite(string name, Uri img, JsonWriter writter)
        {
            if (img == null)
                return;

            writter.WriteStartObject();

            writter.WritePropertyName("size");
            writter.WriteValue(name);

            writter.WritePropertyName("#text");
            writter.WriteValue(img.ToString());

            writter.WriteEndObject();
        }
    }
}
