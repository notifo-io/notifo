// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Microsoft.Extensions.DependencyInjection;

public static class NodaTimeServiceExtensions
{
    public static void AddMyNodaTime(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(SystemClock.Instance);
    }
}
