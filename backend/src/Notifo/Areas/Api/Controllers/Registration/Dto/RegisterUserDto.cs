// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain;
using Notifo.Domain.Integrations;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Registration.Dtos;

public sealed class RegisterUserDto
{
    public bool CreateUser { get; set; }

    public string? EmailAddress { get; set; }

    public string? DisplayName { get; set; }

    public string? PreferredLanguage { get; set; }

    public string? PreferredTimezone { get; set; }

    public TopicId[]? Topics { get; set; }

    public UpsertUser ToUpsert(string userId)
    {
        var result = SimpleMapper.Map(this, new UpsertUser());

        result.FullName = DisplayName;

        result.Settings = new ChannelSettings
        {
            [Providers.WebPush] = new ChannelSetting
            {
                Send = ChannelSend.Send
            }
        };

        if (!string.IsNullOrWhiteSpace(result.EmailAddress))
        {
            result.Settings[Providers.Email] = new ChannelSetting
            {
                Send = ChannelSend.Send
            };
        }

        result.UserId = userId;

        return result;
    }
}
