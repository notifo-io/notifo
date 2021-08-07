// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure;

namespace Notifo.Domain.ChannelTemplates
{
    public sealed class ChannelTemplateStore<T> : IChannelTemplateStore<T>
    {
        private readonly IChannelTemplateRepository<T> repository;
        private readonly IServiceProvider serviceProvider;

        public ChannelTemplateStore(IChannelTemplateRepository<T> repository,
            IServiceProvider serviceProvider)
        {
            this.repository = repository;
            this.serviceProvider = serviceProvider;
        }

        public async Task<IResultList<ChannelTemplate<T>>> QueryAsync(string appId, ChannelTemplateQuery query,
            CancellationToken ct)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNull(query, nameof(query));

            var templates = await repository.QueryAsync(appId, query, ct);

            return templates;
        }

        public async Task<ChannelTemplate<T>?> GetAsync(string appId, string id,
            CancellationToken ct)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNullOrEmpty(id, nameof(id));

            var (template, _) = await repository.GetAsync(appId, id, ct);

            return template;
        }

        public async Task<ChannelTemplate<T>?> GetBestAsync(string appId, string? name,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));

            var template = await repository.GetBestAsync(appId, name, ct);

            return template;
        }

        public Task<ChannelTemplate<T>> UpsertAsync(string appId, string? id, ICommand<ChannelTemplate<T>> command,
            CancellationToken ct)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNull(command, nameof(command));

            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString();
            }

            return Updater.UpdateRetriedAsync(5, async () =>
            {
                var (user, etag) = await repository.GetAsync(appId, id, ct);

                if (user == null)
                {
                    if (!command.CanCreate)
                    {
                        throw new DomainObjectNotFoundException(id);
                    }

                    user = ChannelTemplate<T>.Create(appId, id);
                }

                if (await command.ExecuteAsync(user, serviceProvider, ct))
                {
                    await repository.UpsertAsync(user, etag, ct);

                    await command.ExecutedAsync(user, serviceProvider);
                }

                return user;
            });
        }

        public Task DeleteAsync(string appId, string code,
            CancellationToken ct)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNullOrEmpty(code, nameof(code));

            return repository.DeleteAsync(appId, code, ct);
        }
    }
}
