// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Notifo.Infrastructure;

namespace Notifo.Domain.Log;

[Table("Log")]
[Index(nameof(AppId), nameof(UserId), nameof(LastSeen))]
[Index(nameof(FirstWriteId))]
public sealed class EFLogEntity
{
    private const int HashLength = 64;

    [Key]
    [MaxLength(HashLength)]
    public string Id { get; set; }

    [MaxLength(FieldLengths.AppId)]
    public string AppId { get; set; }

    [MaxLength(FieldLengths.Id)]
    public string? UserId { get; set; }

    [MaxLength(FieldLengths.LongText)]
    public string Message { get; set; }

    [MaxLength(FieldLengths.AppId)]
    public string System { get; set; }

    [MaxLength(FieldLengths.Etag)]
    public string FirstWriteId { get; set; }

    public Instant FirstSeen { get; set; }

    public Instant LastSeen { get; set; }

    public int EventCode { get; set; }

    public long Count { get; set; }

    public static string CreateId(string appId, string? userId, int eventCode, string message, string system)
    {
        // The message can be long, therefore we use a hash as primary key.
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{appId}_{userId}_{eventCode}_{message}_{system}"));

        return Convert.ToHexString(hash);
    }

    public LogEntry ToEntry()
    {
        return new LogEntry
        {
            AppId = AppId,
            Count = Count,
            EventCode = EventCode,
            FirstSeen = FirstSeen,
            FirstWriteId = FirstWriteId,
            LastSeen = LastSeen,
            Message = Message,
            System = System,
            UserId = UserId,
        };
    }
}
