// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Domain;

[JsonConverter(typeof(ChannelSendConverter))]
public enum ChannelSend
{
    Inherit,
    Send,
    NotSending,
    NotAllowed
}

public sealed class ChannelSendConverter : JsonConverter<ChannelSend>
{
    private JsonConverter<ChannelSend>? enumConverter;

    public override ChannelSend Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return ChannelSend.Inherit;
            case JsonTokenType.True:
                return ChannelSend.Send;
            case JsonTokenType.False:
                return ChannelSend.NotSending;
        }

        enumConverter ??= (JsonConverter<ChannelSend>)new JsonStringEnumConverter().CreateConverter(typeof(ChannelSend), options);

        return enumConverter.Read(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, ChannelSend value, JsonSerializerOptions options)
    {
        enumConverter ??= (JsonConverter<ChannelSend>)new JsonStringEnumConverter().CreateConverter(typeof(ChannelSend), options);
        enumConverter.Write(writer, value, options);
    }
}
