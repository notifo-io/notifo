// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Domain.ChannelTemplates;

public sealed class DeleteChannelTemplate<T> : ChannelTemplateCommand<T> where T : class
{
    public override bool IsUpsert => false;

    public override async ValueTask ExecuteAsync(IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        await serviceProvider.GetRequiredService<IChannelTemplateRepository<T>>()
            .DeleteAsync(AppId, TemplateCode, ct);
    }
}
