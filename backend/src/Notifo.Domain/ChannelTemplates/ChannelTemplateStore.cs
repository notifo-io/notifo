// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Notifo.Infrastructure;

namespace Notifo.Domain.ChannelTemplates
{
    public sealed class ChannelTemplateStore<T> : IChannelTemplateStore<T> where T : class
    {
        private readonly IChannelTemplateRepository<T> repository;
        private readonly IServiceProvider serviceProvider;
        private readonly IClock clock;

        public ChannelTemplateStore(IChannelTemplateRepository<T> repository,
            IServiceProvider serviceProvider, IClock clock)
        {
            this.repository = repository;
            this.serviceProvider = serviceProvider;

            this.clock = clock;
        }

        public async Task<IResultList<ChannelTemplate<T>>> QueryAsync(string appId, ChannelTemplateQuery query,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNull(query, nameof(query));

            var templates = await repository.QueryAsync(appId, query, ct);

            return templates;
        }

        public async Task<ChannelTemplate<T>?> GetAsync(string appId, string id,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNullOrEmpty(id, nameof(id));

            var (template, _) = await repository.GetAsync(appId, id, ct);

            return template;
        }

        public async Task<(TemplateResolveStatus, T?)> GetBestAsync(string appId, string? name, string language,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNullOrEmpty(language, nameof(language));

            var template = await repository.GetBestAsync(appId, name, ct);

            if (template == null)
            {
                return (TemplateResolveStatus.NotFound, null);
            }

            if (!template.Languages.TryGetValue(language, out var result))
            {
                return (TemplateResolveStatus.LanguageNotFound, null);
            }

            var status = TemplateResolveStatus.Resolved;

            if (!string.IsNullOrWhiteSpace(name) && !string.Equals(template.Name, name, StringComparison.Ordinal))
            {
                status = TemplateResolveStatus.ResolvedWithFallback;
            }

            return (status, result);
        }

        public Task<ChannelTemplate<T>> UpsertAsync(string appId, string? id, ICommand<ChannelTemplate<T>> command,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNull(command, nameof(command));

            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString();
            }

            return Updater.UpdateRetriedAsync(5, async () =>
            {
                var (template, etag) = await repository.GetAsync(appId, id, ct);

                if (template == null)
                {
                    if (!command.CanCreate)
                    {
                        throw new DomainObjectNotFoundException(id);
                    }

                    template = ChannelTemplate<T>.Create(appId, id);
                }

                var newTemplate = await command.ExecuteAsync(template, serviceProvider, ct);

                if (newTemplate == null || ReferenceEquals(template, newTemplate))
                {
                    return template;
                }

                newTemplate = newTemplate with
                {
                    LastUpdate = clock.GetCurrentInstant()
                };

                await repository.UpsertAsync(newTemplate, etag, ct);

                return newTemplate;
            });
        }

        public Task DeleteAsync(string appId, string id,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNullOrEmpty(id, nameof(id));

            return repository.DeleteAsync(appId, id, ct);
        }
    }
}
