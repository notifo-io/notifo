// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class AppTests : IClassFixture<ClientFixture>
{
    public ClientFixture _ { get; }

    public AppTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_app()
    {
        var appName = Guid.NewGuid().ToString();

        // STEP 0: Create app
        var createRequest = new UpsertAppDto
        {
            Name = appName
        };

        var app_0 = await _.Client.Apps.PostAppAsync(createRequest);

        Assert.Equal(appName, app_0.Name);


        // STEP 1: Query apps.
        var apps = await _.Client.Apps.GetAppsAsync();

        Assert.Equal(appName, apps.FirstOrDefault(x => x.Name == appName)?.Name);


        // STEP 2: Query app.
        var app_1 = await _.Client.Apps.GetAppAsync(app_0.Id);

        Assert.Equal(appName, app_1.Name);

        await Verify(app_1)
            .IgnoreMembersWithType<DateTimeOffset>()
            .IgnoreMembers<AppDto>(x => x.ApiKeys)
            .IgnoreMembers<AppDetailsDto>(x => x.ApiKeys);
    }

    [Fact]
    public async Task Should_update_app_and_manage_integration()
    {
        var appName = Guid.NewGuid().ToString();

        // STEP 0: Create app.
        var app_0 = await _.Client.Apps.PostAppAsync(new UpsertAppDto { Name = appName });


        // STEP 1: Update app.
        var app_1 = await _.Client.Apps.PutAppAsync(app_0.Id, new UpsertAppDto
        {
            Name = appName,
            Languages =
            [
                "en",
                "de"
            ],
            ConfirmUrl = "https://notifo.io/confirm"
        });

        Assert.Equal(new[] { "en", "de" }, app_1.Languages);
        Assert.Equal("https://notifo.io/confirm", app_1.ConfirmUrl);

        var app_2 = await _.Client.Apps.GetAppAsync(app_0.Id);

        Assert.Equal(new[] { "en", "de" }, app_2.Languages);
        Assert.Equal("https://notifo.io/confirm", app_2.ConfirmUrl);


        // STEP 2: Create integration.
        var integration_0 = await _.Client.Apps.PostIntegrationAsync(app_0.Id, new CreateIntegrationDto
        {
            Type = "SMTP",
            Properties = new Dictionary<string, string>
            {
                ["host"] = "localhost",
                ["fromEmail"] = "hello@notifo.io",
                ["fromName"] = "Hello Notifo",
                ["port"] = "1025"
            },
            Enabled = true
        });

        Assert.True(integration_0.Integration.Enabled);


        // STEP 3: Disable the integration.
        await _.Client.Apps.PutIntegrationAsync(app_0.Id, integration_0.Id, new UpdateIntegrationDto
        {
            Properties = integration_0.Integration.Properties,
            Enabled = false
        });

        var integrations_0 = await _.Client.Apps.GetIntegrationsAsync(app_0.Id);

        Assert.False(integrations_0.Configured[integration_0.Id].Enabled);


        // STEP 4: Delete the integration.
        await _.Client.Apps.DeleteIntegrationAsync(app_0.Id, integration_0.Id);

        var integrations_1 = await _.Client.Apps.GetIntegrationsAsync(app_0.Id);

        Assert.DoesNotContain(integration_0.Id, integrations_1.Configured.Keys);
    }

    [Fact]
    public async Task Should_add_and_remove_contributor()
    {
        var appName = Guid.NewGuid().ToString();

        var contributorEmail = $"{Guid.NewGuid()}@notifo.io";

        // STEP 0: Create app.
        var app_0 = await _.Client.Apps.PostAppAsync(new UpsertAppDto { Name = appName });


        // STEP 1: Add contributor.
        var app_1 = await _.Client.Apps.PostContributorAsync(app_0.Id, new AddContributorDto
        {
            Email = contributorEmail,
            Role = "Admin"
        });

        var contributor = app_1.Contributors.SingleOrDefault(x => x.UserName == contributorEmail);

        Assert.NotNull(contributor);
        Assert.Equal("Admin", contributor.Role);


        // STEP 2: Remove contributor.
        var app_2 = await _.Client.Apps.DeleteContributorAsync(app_0.Id, contributor.UserId);

        Assert.DoesNotContain(app_2.Contributors, x => x.UserName == contributorEmail);
    }
}
