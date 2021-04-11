// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable SA1649 // File name should match first type name

namespace Notifo.Domain
{
    [JsonConverter(typeof(NotificationSendConverter))]
    public enum NotificationSend
    {
        Inherit,
        Send,
        NotSending,
        NotAllowed
    }

    public sealed class NotificationSendConverter : JsonConverter<NotificationSend>
    {
        private JsonConverter<NotificationSend>? enumConverter;

        public override NotificationSend Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return NotificationSend.Inherit;
                case JsonTokenType.True:
                    return NotificationSend.Send;
                case JsonTokenType.False:
                    return NotificationSend.NotSending;
            }

            enumConverter ??= (JsonConverter<NotificationSend>)new JsonStringEnumConverter().CreateConverter(typeof(NotificationSend), options);

            return enumConverter.Read(ref reader, typeToConvert, options);
        }

        public override void Write(Utf8JsonWriter writer, NotificationSend value, JsonSerializerOptions options)
        {
            enumConverter ??= (JsonConverter<NotificationSend>)new JsonStringEnumConverter().CreateConverter(typeof(NotificationSend), options);
            enumConverter.Write(writer, value, options);
        }
    }
}
