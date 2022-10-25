// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Notifo.Infrastructure.Collections.Json;
using Notifo.Infrastructure.Json;
using Squidex.Messaging;
using Squidex.Messaging.Implementation;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JsonServiceExtensions
    {
        public static void AddMyJson(this IServiceCollection services, Action<JsonSerializerOptions> configure)
        {
            var options =
                new JsonSerializerOptions()
                    .Configure(configure);

            services.AddSingleton(options);

            services.AddSingletonAs(c => new SystemTextJsonMessagingSerializer(c => Configure(c, configure)))
                .As<IMessagingSerializer>();

            services.AddSingletonAs<SystemTextJsonSerializer>()
                .As<IJsonSerializer>();
        }

        public static JsonSerializerOptions Configure(this JsonSerializerOptions options, Action<JsonSerializerOptions> configure)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            options.Converters.Add(new JsonActivityContextConverter());
            options.Converters.Add(new JsonActivitySpanIdConverter());
            options.Converters.Add(new JsonActivityTraceIdConverter());
            options.Converters.Add(new JsonInstantConverter());
            options.Converters.Add(new JsonReadonlyDictionaryConverterFactory());
            options.Converters.Add(new JsonReadonlyListConverterFactory());
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new JsonTimeSpanConverter());
            options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            configure(options);

            return options;
        }
    }
}
