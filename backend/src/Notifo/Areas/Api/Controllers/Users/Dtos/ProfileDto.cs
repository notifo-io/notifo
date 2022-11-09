// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Users.Dtos;

public sealed class ProfileDto
{
    /// <summary>
    /// The full name of the user.
    /// </summary>
    [Required]
    public string FullName { get; set; }

    /// <summary>
    /// The email of the user.
    /// </summary>
    [Required]
    public string EmailAddress { get; set; }

    /// <summary>
    /// The phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// The allowed Topics.
    /// </summary>
    public ReadonlyList<string> AllowedTopics { get; set; }

    /// <summary>
    /// The preferred language of the user.
    /// </summary>
    public string? PreferredLanguage { get; set; }

    /// <summary>
    /// The timezone of the user.
    /// </summary>
    public string? PreferredTimezone { get; set; }

    /// <summary>
    /// The supported languages.
    /// </summary>
    [Required]
    public string[] SupportedLanguages { get; set; }

    /// <summary>
    /// The supported timezones.
    /// </summary>
    [Required]
    public string[] SupportedTimezones { get; set; }

    /// <summary>
    /// Notification settings per channel.
    /// </summary>
    [Required]
    public Dictionary<string, ChannelSettingDto> Settings { get; set; } = new Dictionary<string, ChannelSettingDto>();

    public static ProfileDto FromDomainObject(User source, App app)
    {
        var result = SimpleMapper.Map(source, new ProfileDto());

        result.SupportedTimezones = DateTimeZoneProviders.Tzdb.Ids.ToArray();
        result.SupportedLanguages = app.Languages.ToArray();

        if (source.Settings != null)
        {
            foreach (var (key, value) in source.Settings)
            {
                if (value != null)
                {
                    result.Settings[key] = ChannelSettingDto.FromDomainObject(value);
                }
            }
        }

        return result;
    }
}
