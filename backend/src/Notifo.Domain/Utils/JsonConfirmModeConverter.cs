// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Notifo.Domain.Utils
{
    public sealed class JsonConfirmModeConverter : JsonConverter<ConfirmMode>
    {
        public override ConfirmMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var text = reader.GetString();

            if (Enum.TryParse<ConfirmMode>(text, true, out var confirmMode))
            {
                return confirmMode;
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, ConfirmMode value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
