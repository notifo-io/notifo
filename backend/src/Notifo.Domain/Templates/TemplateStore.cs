// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Mediator;

namespace Notifo.Domain.Templates;

public sealed class TemplateStore : ITemplateStore, IRequestHandler<TemplateCommand, Template?>
{
    private readonly ITemplateRepository repository;
    private readonly IServiceProvider serviceProvider;

    public TemplateStore(ITemplateRepository repository,
        IServiceProvider serviceProvider)
    {
        this.repository = repository;
        this.serviceProvider = serviceProvider;
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

        if (template == null)
        {
            template = new Template(appId, code, SystemClock.Instance.GetCurrentInstant())
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

    public async ValueTask<Template?> HandleAsync(TemplateCommand command,
        CancellationToken ct)
    {
        Guard.NotNullOrEmpty(command.AppId);
        Guard.NotNullOrEmpty(command.TemplateCode);

        if (!command.IsUpsert)
        {
            await command.ExecuteAsync(serviceProvider, ct);
            return null!;
        }

        return await Updater.UpdateRetriedAsync(5, async () =>
        {
            var (template, etag) = await repository.GetAsync(command.AppId, command.TemplateCode, ct);

            if (template == null)
            {
                if (!command.CanCreate)
                {
                    throw new DomainObjectNotFoundException(command.TemplateCode);
                }

                template = new Template(command.AppId, command.TemplateCode, command.Timestamp);
            }

            var newTemplate = await command.ExecuteAsync(template, serviceProvider, ct);

            if (newTemplate != null && !ReferenceEquals(newTemplate, template))
            {
                newTemplate = newTemplate with
                {
                    LastUpdate = command.Timestamp
                };

                await repository.UpsertAsync(newTemplate, etag, ct);
                template = newTemplate;

                await command.ExecutedAsync(serviceProvider);
            }

            return newTemplate;
        });
    }
}
