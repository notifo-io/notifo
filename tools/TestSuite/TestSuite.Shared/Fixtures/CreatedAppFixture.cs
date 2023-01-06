// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;

namespace TestSuite.Fixtures;

public class CreatedAppFixture : ClientFixture
{
    public AppDto App { get; private set; }

    public string AppId => App.Id;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        App = await Factories.CreateAsync(nameof(AppName), async () =>
        {
            var app = await FindAppAsync();

            if (app != null)
            {
                return app;
            }

            try
            {
                var createRequest = new UpsertAppDto
                {
                    Name = AppName,
                    Languages = new List<string>
                    {
                        "en",
                        "de"
                    }
                };

                await Client.Apps.PostAppAsync(createRequest);
            }
            catch (NotifoException ex)
            {
                if (ex.StatusCode != 400)
                {
                    throw;
                }
            }

            app = await FindAppAsync();

            return app!;
        });

        await CreateContributorAsync("sebastian@squidex.io");
        await CreateFirebaseAsync();
    }

    private async Task CreateContributorAsync(string email)
    {
        var app = await Client.Apps.GetAppAsync(AppId);

        if (app.Contributors.Any(x => x.UserName == email))
        {
            return;
        }

        var request = new AddContributorDto
        {
            Role = "Owner", Email = email
        };

        await Client.Apps.PostContributorAsync(AppId, request);
    }

    private async Task CreateFirebaseAsync()
    {
        var integrations = await Client.Apps.GetIntegrationsAsync(AppId);

        if (integrations.Configured.Any(x => x.Value.Type == "Firebase"))
        {
            return;
        }

        var request = new CreateIntegrationDto
        {
            Type = "Firebase",
            Properties = new Dictionary<string, string>
            {
                ["projectId"] = "PROJECT",
                ["silentAndroid"] = "false",
                ["silentIOS"] = "false",
                ["credentials"] = "CREDENTIALS"
            },
            Enabled = true
        };

        await Client.Apps.PostIntegrationAsync(AppId, request);
    }

    private async Task<AppDto?> FindAppAsync()
    {
        var apps = await Client.Apps.GetAppsAsync();

        return apps.FirstOrDefault(x => x.Name == AppName);
    }

    public INotifoClient GetClient(ClientMode mode)
    {
        if (mode == ClientMode.ClientId)
        {
            return Client;
        }
        else
        {
            return BuildAppClient(App);
        }
    }
}
