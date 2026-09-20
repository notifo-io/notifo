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
using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Notifo.Infrastructure.ObjectPool;

namespace Notifo.Domain.Events;

[Table("Events")]
[Index(nameof(AppId), nameof(Created))]
[Index(nameof(Created))]
public sealed class EFEventEntity : EFEntity<Event>, IEFCounterEntity
{
    [MaxLength(FieldLengths.AppId)]
    public string AppId { get; set; }

    [MaxLength(FieldLengths.Key)]
    public string Topic { get; set; }

    [MaxLength(FieldLengths.LongText)]
    public string SearchText { get; set; }

    [MaxLength(FieldLengths.Key)]
    public string SendChannels { get; set; }

    [Json]
    [MaxLength(FieldLengths.LongText)]
    public CounterMap? Counters { get; set; }

    public Instant Created { get; set; }

    public bool Pending { get; set; }

    public long CountersVersion { get; set; }

    public static string CreateId(string appId, string id)
    {
        return $"{appId}_{id}";
    }

    public static EFEventEntity FromEvent(Event @event)
    {
        return new EFEventEntity
        {
            AppId = @event.AppId,
            Counters = @event.Counters,
            CountersVersion = 0,
            Created = @event.Created,
            Doc = @event,
            DocId = CreateId(@event.AppId, @event.Id),
            Etag = GenerateEtag(),
            Pending = true,
            SearchText = BuildSearchText(@event).ToMaxLength(FieldLengths.LongText)!,
            SendChannels = @event.Settings.Where(x => x.Value.Send is ChannelSend.Send).Select(x => x.Key).ToTags(),
            Topic = @event.Topic.ToMaxLength(FieldLengths.Key)!,
        };
    }

    private static string BuildSearchText(Event @event)
    {
        var sb = DefaultPools.StringBuilder.Get();
        try
        {
            foreach (var text in @event.Formatting.Subject.Values)
            {
                sb.AppendLine(text);
            }

            if (@event.Formatting.Body != null)
            {
                foreach (var text in @event.Formatting.Body.Values)
                {
                    sb.AppendLine(text);
                }
            }

            return sb.ToString();
        }
        finally
        {
            DefaultPools.StringBuilder.Return(sb);
        }
    }

    public Event ToEvent()
    {
        var @event = Doc;

        @event.Counters = Counters;

        return @event;
    }
}
