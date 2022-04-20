// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;

namespace TestSuite.Fixtures
{
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
                    await Client.Apps.PostAppAsync(new UpsertAppDto
                    {
                        Name = AppName,
                        Languages = new[]
                        {
                            "en",
                            "de"
                        }
                    });
                }
                catch (NotifoException ex)
                {
                    if (ex.StatusCode != 400)
                    {
                        throw;
                    }
                }

                appId = await FindAppAsync();

                string[] contributors =
                {
                    "hello@squidex.io"
                };

                var invite = new AddContributorDto { Role = "Owner" };

                foreach (var contributor in contributors)
                {
                    invite.Email = contributor;

                    await Client.Apps.PostContributorAsync(appId, invite);
                }

                return appId;
            });
        }

        private async Task<string> FindAppAsync()
        {
            var apps = await Client.Apps.GetAppsAsync();

            return apps.FirstOrDefault(x => x.Name == AppName)?.Id;
        }
    }
}
