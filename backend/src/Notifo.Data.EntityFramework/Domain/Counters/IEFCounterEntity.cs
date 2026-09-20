// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Counters;

public interface IEFCounterEntity
{
    string DocId { get; }

    CounterMap? Counters { get; set; }

    long CountersVersion { get; set; }
}
