// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Notifo.Domain.Apps;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Mediator;

namespace Notifo.Domain.Integrations;

public class IntegrationManagerTests
{
    private readonly IAppStore appStore = A.Fake<IAppStore>();
    private readonly IIntegration integration = A.Fake<IIntegration>();
    private readonly IIntegrationRegistry integrationRegistry = A.Fake<IIntegrationRegistry>();
    private readonly IMediator mediator = A.Fake<IMediator>();
    private readonly IntegrationManager sut;

    public IntegrationManagerTests()
    {
        var found = integration;

        A.CallTo(() => integrationRegistry.TryGetIntegration("my-type", out found))
            .Returns(true)
            .AssignsOutAndRefParameters(integration);

        A.CallTo(() => integration.Definition)
            .Returns(new IntegrationDefinition("my-type", "My Type", string.Empty, [], [], new HashSet<string>()));

        var app = new App("app", default)
        {
            Integrations = new Dictionary<string, ConfiguredIntegration>
            {
                ["integration1"] = new ConfiguredIntegration("my-type", ReadonlyDictionary.Empty<string, string>())
                {
                    Status = IntegrationStatus.Pending
                }
            }.ToReadonlyDictionary()
        };

        A.CallTo(() => appStore.QueryWithPendingIntegrationsAsync(A<CancellationToken>._))
            .Returns(new List<App> { app });

        sut = new IntegrationManager(
            appStore,
            A.Fake<IIntegrationAdapter>(),
            [integrationRegistry],
            A.Fake<IIntegrationUrl>(),
            A.Fake<IServiceProvider>(),
            mediator,
            A.Fake<ILogger<IntegrationManager>>());
    }

    [Fact]
    public async Task Should_update_status_of_pending_integration()
    {
        A.CallTo(() => integration.CheckStatusAsync(A<IntegrationContext>._, A<CancellationToken>._))
            .Returns(IntegrationStatus.Verified);

        await sut.CheckAsync(default);

        A.CallTo(() => mediator.SendAsync(
                A<UpdateAppIntegrationStatus>.That.Matches(x => x.Status["integration1"] == IntegrationStatus.Verified),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_update_status_if_integration_is_still_pending()
    {
        A.CallTo(() => integration.CheckStatusAsync(A<IntegrationContext>._, A<CancellationToken>._))
            .Returns(IntegrationStatus.Pending);

        await sut.CheckAsync(default);

        A.CallTo(() => mediator.SendAsync<App?>(A<UpdateAppIntegrationStatus>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_update_status_if_check_failed()
    {
        A.CallTo(() => integration.CheckStatusAsync(A<IntegrationContext>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException());

        await sut.CheckAsync(default);

        A.CallTo(() => mediator.SendAsync<App?>(A<UpdateAppIntegrationStatus>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
