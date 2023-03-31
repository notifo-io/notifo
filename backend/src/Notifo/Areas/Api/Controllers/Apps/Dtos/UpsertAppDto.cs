// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Areas.Api.OpenApi;
using Notifo.Domain.Apps;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos;

[OpenApiRequest]
public sealed class UpsertAppDto
{
    /// <summary>
    /// The app name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The supported languages.
    /// </summary>
    public string[]? Languages { get; set; }

    /// <summary>
    /// The sender email address.
    /// </summary>
    public string? EmailAddress { get; set; }

    /// <summary>
    /// The sender email name.
    /// </summary>
    public string? EmailName { get; set; }

    /// <summary>
    /// The firebase project ID.
    /// </summary>
    public string? FirebaseProject { get; set; }

    /// <summary>
    /// The firebase credentials.
    /// </summary>
    public string? FirebaseCredential { get; set; }

    /// <summary>
    /// The webhook URL.
    /// </summary>
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// The confirm URL.
    /// </summary>
    public string? ConfirmUrl { get; set; }

    /// <summary>
    /// True, when emails are allowed.
    /// </summary>
    public bool? AllowEmail { get; set; }

    /// <summary>
    /// True, when SMS are allowed.
    /// </summary>
    public bool? AllowSms { get; set; }

    public UpsertApp ToUpsert()
    {
        return SimpleMapper.Map(this, new UpsertApp());
    }
}
