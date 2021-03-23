// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Notifo.SDK
{
    public partial class ErrorDto
    {
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
        public override string ToString()
        {
            return $"{Result}\n{base.ToString()}";
        }
    }

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
