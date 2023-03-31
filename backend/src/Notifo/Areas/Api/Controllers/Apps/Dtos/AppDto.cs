// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos;

public sealed class AppDto
{
    private static readonly Dictionary<string, long> EmptyCounters = new Dictionary<string, long>();

    /// <summary>
    /// The id of the app.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The app name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The current role.
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    /// The date time (ISO 8601) when the app has been created.
    /// </summary>
    public Instant Created { get; set; }

    /// <summary>
    /// The date time (ISO 8601) when the app has been updated.
    /// </summary>
    public Instant LastUpdate { get; set; }

    /// <summary>
    /// The supported languages.
    /// </summary>
    public ReadonlyList<string> Languages { get; set; }

    /// <summary>
    /// The api keys.
    /// </summary>
    public ReadonlyDictionary<string, string> ApiKeys { get; set; }

    /// <summary>
    /// The statistics counters.
    /// </summary>
    public Dictionary<string, long> Counters { get; set; }

    public static AppDto FromDomainObject(App source, string userId)
    {
        var result = SimpleMapper.Map(source, new AppDto());

        if (userId != null && source.Contributors.TryGetValue(userId, out var userRole))
        {
            result.Role = userRole;
        }

        result.Counters = source.Counters ?? EmptyCounters;

        return result;
    }
}
