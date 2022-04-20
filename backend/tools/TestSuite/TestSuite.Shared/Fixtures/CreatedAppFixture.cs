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
        private static readonly string[] Contributors =
        {
            "hello@squidex.io"
        };

        public string AppId { get; set; }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            if (AppId == null)
            {
                try
                {
                    var app = await Client.Apps.PostAppAsync(new UpsertAppDto
                    {
                        Name = AppName,
                        Languages = new[]
                        {
                            "en",
                            "de"
                        }
                    });

                    AppId = app.Id;
                }
                catch (NotifoException ex)
                {
                    if (ex.StatusCode != 400)
                    {
                        throw;
                    }
                }
            }

            if (AppId == null)
            {
                await FindAppAsync();
            }

            var invite = new AddContributorDto { Role = "Owner" };

            foreach (var contributor in Contributors)
            {
                invite.Email = contributor;

                await Client.Apps.PostContributorAsync(AppId, invite);
            }
        }

        private async Task FindAppAsync()
        {
            var apps = await Client.Apps.GetAppsAsync();

            AppId = apps.FirstOrDefault(x => x.Name == AppName)?.Id;
        }
    }
}
