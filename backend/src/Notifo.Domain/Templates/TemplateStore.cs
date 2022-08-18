// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Infrastructure;

namespace Notifo.Domain.Templates
{
    public sealed class TemplateStore : ITemplateStore
    {
        private readonly ITemplateRepository repository;
        private readonly IServiceProvider services;
        private readonly IClock clock;

        public TemplateStore(ITemplateRepository repository,
            IServiceProvider services, IClock clock)
        {
            this.repository = repository;
            this.services = services;
            this.clock = clock;
        }

        public async Task<IResultList<Template>> QueryAsync(string appId, TemplateQuery query,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNull(query);

            var templates = await repository.QueryAsync(appId, query, ct);

            return templates;
        }

        public async Task<Template?> GetAsync(string appId, string code,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNullOrEmpty(code);

            var (template, _) = await repository.GetAsync(appId, code, ct);

            // Calculate once to have some timestamp for created and updated when new entity is created.
            var now = clock.GetCurrentInstant();

            if (template == null)
            {
                template = new Template(appId, code, now)
                {
                    IsAutoCreated = true
                };

                try
                {
                    await repository.UpsertAsync(template, null, ct);
                }
                catch
                {
                    return null;
                }
            }

            return template;
        }

        public Task<Template> UpsertAsync(string appId, string code, ICommand<Template> command,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNullOrEmpty(code);
            Guard.NotNull(command);

            return Updater.UpdateRetriedAsync(5, async () =>
            {
                var (template, etag) = await repository.GetAsync(appId, code, ct);

                // Calculate once to have some timestamp for created and updated when new entity is created.
                var now = clock.GetCurrentInstant();

                if (template == null)
                {
                    if (!command.CanCreate)
                    {
                        throw new DomainObjectNotFoundException(code);
                    }

                    template = new Template(appId, code, now);
                }

                var newTemplate = await command.ExecuteAsync(template, services, ct);

                if (newTemplate == null || ReferenceEquals(newTemplate, template))
                {
                    return template;
                }

                newTemplate = newTemplate with
                {
                    LastUpdate = now
                };

                await repository.UpsertAsync(newTemplate, etag, ct);

                return newTemplate;
            });
        }

        public Task DeleteAsync(string appId, string code,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId);
            Guard.NotNullOrEmpty(code);

            return repository.DeleteAsync(appId, code, ct);
        }
    }
}
