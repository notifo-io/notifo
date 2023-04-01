// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Areas.Api.OpenApi;
using Notifo.Domain;
using Notifo.Domain.Subscriptions;

namespace Notifo.Areas.Api.Controllers.Users.Dtos;

[OpenApiRequest]
public sealed class SubscribeDto
{
    /// <summary>
    /// The topic to add.
    /// </summary>
    [Required]
    public string TopicPrefix { get; set; }

    /// <summary>
    /// Notification settings per channel.
    /// </summary>
    public Dictionary<string, ChannelSettingDto>? TopicSettings { get; set; }

    /// <summary>
    /// The scheduling settings.
    /// </summary>
    public SchedulingDto? Scheduling { get; set; }

    /// <summary>
    /// Indicates whether scheduling should be overriden.
    /// </summary>
    public bool HasScheduling { get; set; }

    public Subscribe ToUpdate(string userId)
    {
        var result = new Subscribe
        {
            Topic = new TopicId(TopicPrefix)
        };

        if (TopicSettings?.Any() == true)
        {
            result.TopicSettings = new ChannelSettings();

            foreach (var (key, value) in TopicSettings)
            {
                if (value != null)
                {
                    result.TopicSettings[key] = value.ToDomainObject();
                }
            }
        }

        result.Scheduling = Scheduling?.ToDomainObject();
        result.HasScheduling = HasScheduling;
        result.UserId = userId;

        return result;
    }
}
