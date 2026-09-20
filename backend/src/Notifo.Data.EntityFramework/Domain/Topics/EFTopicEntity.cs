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

namespace Notifo.Domain.Topics;

[Table("Topics")]
[Index(nameof(AppId), nameof(LastUpdate))]
public sealed class EFTopicEntity : EFEntity<Topic>, IEFCounterEntity
{
    [MaxLength(FieldLengths.AppId)]
    public string AppId { get; set; }

    [MaxLength(FieldLengths.Key)]
    public string Path { get; set; }

    [Json]
    [MaxLength(FieldLengths.LongText)]
    public CounterMap? Counters { get; set; }

    public bool IsExplicit { get; set; }

    public Instant LastUpdate { get; set; }

    public long CountersVersion { get; set; }

    public static string CreateId(string appId, string path)
    {
        return $"{appId}_{path}";
    }

    public static EFTopicEntity FromTopic(Topic topic)
    {
        return new EFTopicEntity
        {
            AppId = topic.AppId,
            Counters = topic.Counters,
            CountersVersion = 0,
            Doc = topic,
            DocId = CreateId(topic.AppId, topic.Path),
            Etag = GenerateEtag(),
            IsExplicit = topic.IsExplicit,
            LastUpdate = topic.LastUpdate,
            Path = topic.Path,
        };
    }

    public Topic ToTopic()
    {
        return Doc with { Counters = Counters ?? [] };
    }
}
