// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;

namespace Notifo.SDK
{
    public partial class AppsClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            settings.Converters.Add(DateTimeOffsetConverter.Instance);
        }
    }

    public partial class ConfigsClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            settings.Converters.Add(DateTimeOffsetConverter.Instance);
        }
    }

    public partial class EventsClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            settings.Converters.Add(DateTimeOffsetConverter.Instance);
        }
    }

    public partial class LogsClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            settings.Converters.Add(DateTimeOffsetConverter.Instance);
        }
    }

    public partial class MediaClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            settings.Converters.Add(DateTimeOffsetConverter.Instance);
        }
    }

    public partial class TemplatesClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            settings.Converters.Add(DateTimeOffsetConverter.Instance);
        }
    }

    public partial class TopicsClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            settings.Converters.Add(DateTimeOffsetConverter.Instance);
        }
    }

    public partial class UsersClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            settings.Converters.Add(DateTimeOffsetConverter.Instance);
        }
    }
}
