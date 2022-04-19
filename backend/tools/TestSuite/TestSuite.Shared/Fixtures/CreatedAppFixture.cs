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

        public static string AppId { get; set; }

        public CreatedAppFixture()
        {
            if (AppId == null)
            {
                Task.Run(async () =>
                {
                    var apps = await Client.Apps.GetAppsAsync();

                    AppId = apps.FirstOrDefault(x => x.Name == AppName)?.Id;
                }).Wait();
            }

            if (AppId == null)
            {
                Task.Run(async () =>
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

                    var invite = new AddContributorDto { Role = "Owner" };

                    foreach (var contributor in Contributors)
                    {
                        invite.Email = contributor;

                        await Client.Apps.PostContributorAsync(AppName, invite);
                    }
                }).Wait();
            }
        }
    }
}
