// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Notifo.Domain.Counters;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure;

namespace Notifo.Domain.Apps;

[Table("Apps")]
[Index(nameof(AuthDomain))]
[Index(nameof(IsPending))]
public sealed class EFAppEntity : EFEntity<App>, IEFCounterEntity
{
    [MaxLength(FieldLengths.Key)]
    public string? AuthDomain { get; set; }

    [Json]
    [MaxLength(FieldLengths.LongText)]
    public CounterMap? Counters { get; set; }

    public bool IsPending { get; set; }

    public long CountersVersion { get; set; }

    public static EFAppEntity FromApp(App app)
    {
        return new EFAppEntity
        {
            AuthDomain = app.AuthScheme?.Domain,
            Counters = app.Counters,
            CountersVersion = 0,
            Doc = app,
            DocId = app.Id,
            Etag = GenerateEtag(),
            IsPending = app.Integrations.Values.Any(x => x.Status is IntegrationStatus.Pending),
        };
    }

    public App ToApp()
    {
        return Doc with { Counters = Counters ?? [] };
    }
}
