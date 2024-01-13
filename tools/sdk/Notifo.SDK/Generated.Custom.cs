// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Newtonsoft.Json;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable MA0048 // File name must match type name

namespace Notifo.SDK;

public partial class ErrorDto
{
    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();

        var message = Message.Trim();

        if (!string.IsNullOrWhiteSpace(message))
        {
            sb.Append(message);
        }

        var detailAdded = false;

        if (Details != null)
        {
            var validDetails = Details.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim());

            if (validDetails.Any())
            {
                if (!message.EndsWith('.') &&
                    !message.EndsWith(':') &&
                    !message.EndsWith(','))
                {
                    sb.Append(':');
                }

                foreach (var detail in validDetails)
                {
                    sb.Append(detail);

                    if (!detail.EndsWith('.'))
                    {
                        sb.Append('.');
                    }

                    detailAdded = true;
                }
            }
        }

        if (!detailAdded && !message.EndsWith('.'))
        {
            sb.Append('.');
        }

        return sb.ToString();
    }
}

#pragma warning disable RECS0096 // Type parameter is never used
public partial class NotifoException<TResult>
#pragma warning restore RECS0096 // Type parameter is never used
{
    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Result}\n{base.ToString()}";
    }
}

public partial class AppsClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.Configure();
    }
}

public partial class ConfigsClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.Configure();
    }
}

public partial class EmailTemplatesClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.Configure();
    }
}

public partial class EventsClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.Configure();
    }
}

public partial class LogsClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.Configure();
    }
}

public partial class MobilePushClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.Configure();
    }
}

public partial class MediaClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.Configure();
    }
}

public partial class NotificationsClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.Configure();
    }
}

public partial class PingClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.Configure();
    }
}

public partial class SmsTemplatesClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.Configure();
    }
}

public partial class SystemUsersClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.Configure();
    }
}

public partial class TemplatesClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.Configure();
    }
}

public partial class TopicsClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.Configure();
    }
}

public partial class UserClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.Configure();
    }
}

public partial class UsersClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.Configure();
    }
}
