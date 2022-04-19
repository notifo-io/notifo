// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using TestSuite.Fixtures;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
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

            Assert.Equal(app_0.Name, appName);


            // STEP 1: Query apps.
            var apps = await _.Client.Apps.GetAppsAsync();

            Assert.Equal(appName, apps.FirstOrDefault(x => x.Name == appName)?.Name);


            // STEP 2: Query app.
            var app_1 = await _.Client.Apps.GetAppAsync(app_0.Id);

            Assert.Equal(app_1.Name, appName);
        }
    }
}
