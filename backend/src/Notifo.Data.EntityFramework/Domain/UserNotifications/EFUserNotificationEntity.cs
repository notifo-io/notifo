// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Notifo.Infrastructure;

namespace Notifo.Domain.UserNotifications;

[Table("UserNotifications")]
[Index(nameof(AppId), nameof(UserId), nameof(Updated), nameof(IsDeleted), nameof(Created))]
[Index(nameof(AppId), nameof(CorrelationId), nameof(Updated), nameof(IsDeleted), nameof(Created))]
[Index(nameof(Created))]
public sealed class EFUserNotificationEntity : EFEntity<UserNotification>
{
    [MaxLength(FieldLengths.AppId)]
    public string AppId { get; set; }

    [MaxLength(FieldLengths.Id)]
    public string UserId { get; set; }

    [MaxLength(FieldLengths.Id)]
    public string? CorrelationId { get; set; }

    [MaxLength(FieldLengths.Text)]
    public string? Subject { get; set; }

    [MaxLength(FieldLengths.Key)]
    public string SendChannels { get; set; }

    public Instant Created { get; set; }

    public Instant Updated { get; set; }

    public bool IsDeleted { get; set; }

    public static string CreateId(Guid id)
    {
        return id.ToString();
    }

    public static EFUserNotificationEntity FromNotification(UserNotification notification)
    {
        return new EFUserNotificationEntity
        {
            AppId = notification.AppId,
            CorrelationId = notification.CorrelationId.ToMaxLength(FieldLengths.Id),
            Created = notification.Created,
            Doc = notification,
            DocId = CreateId(notification.Id),
            Etag = GenerateEtag(),
            IsDeleted = notification.IsDeleted,
            SendChannels = notification.Channels.Where(x => x.Value.Setting.Send is ChannelSend.Send).Select(x => x.Key).ToTags(),
            Subject = notification.Formatting?.Subject.ToMaxLength(FieldLengths.Text),
            Updated = notification.Updated,
            UserId = notification.UserId,
        };
    }

    public UserNotification ToNotification()
    {
        var notification = Doc;

        // The deleted flag is only updated in the column.
        notification.IsDeleted = IsDeleted;

        return notification;
    }
}
