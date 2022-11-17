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
    public string AppId { get; private set; }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        AppId = await Factories.CreateAsync(nameof(AppName), async () =>
        {
            var appId = await FindAppAsync();

            if (appId != null)
            {
                return appId;
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

            appId = await FindAppAsync();

            return appId;
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

    private async Task<string> FindAppAsync()
    {
        var apps = await Client.Apps.GetAppsAsync();

        return apps.FirstOrDefault(x => x.Name == AppName)?.Id;
    }
}
