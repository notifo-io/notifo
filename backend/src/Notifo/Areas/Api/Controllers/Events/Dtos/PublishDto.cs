// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using NodaTime;
using Notifo.Domain;
using Notifo.Domain.Events;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Events.Dtos;

public sealed class PublishDto
{
    /// <summary>
    /// The topic path.
    /// </summary>
    [Required]
    public string Topic { get; set; }

    /// <summary>
    /// A custom id to identity the creator.
    /// </summary>
    public string? CreatorId { get; set; }

    /// <summary>
    /// The template code.
    /// </summary>
    public string? TemplateCode { get; set; }

    /// <summary>
    /// The template variants with propability.
    /// </summary>
    public Dictionary<string, double>? TemplateVariants { get; set; }

    /// <summary>
    /// Additional user defined data.
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// A custom timestamp.
    /// </summary>
    public Instant Timestamp { get; set; }

    /// <summary>
    /// Preformatting when no template is used.
    /// </summary>
    public NotificationFormattingDto? Preformatted { get; set; }

    /// <summary>
    /// The notification settings.
    /// </summary>
    public Dictionary<string, ChannelSettingDto>? Settings { get; set; }

    /// <summary>
    /// User defined properties.
    /// </summary>
    public NotificationProperties? Properties { get; set; }

    /// <summary>
    /// The scheduling options.
    /// </summary>
    public SchedulingDto? Scheduling { get; set; }

    /// <summary>
    /// True when silent.
    /// </summary>
    public bool Silent { get; set; }

    /// <summary>
    /// True when using test integrations.
    /// </summary>
    public bool Test { get; set; }

    /// <summary>
    /// The time to live in seconds.
    /// </summary>
    public int? TimeToLiveInSeconds { get; set; }

    public EventMessage ToEvent(string appId, string? topic = null)
    {
        var result = SimpleMapper.Map(this, new EventMessage());

        if (Preformatted != null)
        {
            result.Formatting = Preformatted.ToDomainObject();
        }

        if (Settings?.Any() == true)
        {
            result.Settings = new ChannelSettings();

            foreach (var (key, value) in Settings)
            {
                if (value != null)
                {
                    result.Settings[key] = value.ToDomainObject();
                }
            }
        }

        if (Scheduling != null)
        {
            result.Scheduling = Scheduling.ToDomainObject();
        }
        else
        {
            result.Scheduling = new Scheduling();
        }

        if (topic != null)
        {
            result.Topic = topic;
        }

        result.AppId = appId;

        return result;
    }
}
