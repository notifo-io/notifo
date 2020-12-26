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
using Notifo.Infrastructure.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JsonServiceExtensions
    {
        public static void AddMyJson(this IServiceCollection services)
        {
            var options =
                new JsonSerializerOptions()
                    .Configure();

            services.AddSingleton(options);

            services.AddSingletonAs<SystemTextJsonSerializer>()
                .As<IJsonSerializer>();
        }

        public static JsonSerializerOptions Configure(this JsonSerializerOptions options)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new JsonTimeSpanConverter());
            options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            return options;
        }
    }
}
