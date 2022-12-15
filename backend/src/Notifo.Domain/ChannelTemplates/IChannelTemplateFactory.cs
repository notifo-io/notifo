// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.ChannelTemplates;

public interface IChannelTemplateFactory<T>
{
    ValueTask<T> CreateInitialAsync(
        CancellationToken ct = default);

    ValueTask ParseAsync(T input, bool strict,
        CancellationToken ct = default);
}
