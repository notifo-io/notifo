// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure;

namespace Notifo.Domain.Templates
{
    public sealed class TemplateStore : ITemplateStore
    {
        private readonly ITemplateRepository repository;

        public TemplateStore(ITemplateRepository repository)
        {
            this.repository = repository;
        }

        public async Task<IResultList<Template>> QueryAsync(string appId, TemplateQuery query, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNull(query, nameof(query));

            var templates = await repository.QueryAsync(appId, query, ct);

            return templates;
        }

        public async Task<Template?> GetAsync(string appId, string code, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNullOrEmpty(code, nameof(code));

            var (template, _) = await repository.GetAsync(appId, code, ct);

            if (template == null)
            {
                template = Template.Create(appId, code);
                template.IsAutoCreated = true;

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

        public Task<Template> UpsertAsync(string appId, string code, TemplateUpdate update, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNullOrEmpty(code, nameof(code));
            Guard.NotNull(update, nameof(update));

            return Updater.UpdateRetriedAsync(5, async () =>
            {
                var (template, etag) = await repository.GetAsync(appId, code, ct);

                if (template == null)
                {
                    template = Template.Create(appId, code);
                }

                template.Update(update);

                await repository.UpsertAsync(template, etag, ct);

                return template;
            });
        }

        public Task DeleteAsync(string appId, string code, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNullOrEmpty(code, nameof(code));

            return repository.DeleteAsync(appId, code, ct);
        }
    }
}
