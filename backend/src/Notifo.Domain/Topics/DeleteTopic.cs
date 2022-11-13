// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Domain.Topics;

public sealed class DeleteTopic : TopicCommand
{
    public override bool IsUpsert => false;

    public override async ValueTask ExecuteAsync(IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        await serviceProvider.GetRequiredService<ITopicRepository>()
            .DeleteAsync(AppId, Path, ct);
    }
}
