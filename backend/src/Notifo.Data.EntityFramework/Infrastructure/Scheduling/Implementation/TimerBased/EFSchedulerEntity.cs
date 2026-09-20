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

namespace Notifo.Infrastructure.Scheduling.Implementation.TimerBased;

[Table("Scheduler")]
[Index(nameof(QueueName), nameof(GroupKey), nameof(Progressing), nameof(DueTime))]
[Index(nameof(QueueName), nameof(Progressing), nameof(DueTime))]
public sealed class EFSchedulerEntity
{
    [Key]
    [MaxLength(FieldLengths.Etag)]
    public string Id { get; set; }

    [MaxLength(FieldLengths.AppId)]
    public string QueueName { get; set; }

    [MaxLength(FieldLengths.Key)]
    public string GroupKey { get; set; }

    [Json]
    public string Jobs { get; set; }

    public bool Progressing { get; set; }

    public Instant? ProgressingStarted { get; set; }

    public Instant DueTime { get; set; }

    public int RetryCount { get; set; }

    public long Version { get; set; }
}
