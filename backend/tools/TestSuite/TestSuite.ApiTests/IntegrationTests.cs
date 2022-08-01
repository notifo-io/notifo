// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.ApiTests
{
    [UsesVerify]
    public class IntegrationTests : IClassFixture<CreatedAppFixture>
    {
        public CreatedAppFixture _ { get; set; }

        public IntegrationTests(CreatedAppFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_create_integration()
        {
            // STEP 1: Create integration
            var emailIntegrationRequest = new CreateIntegrationDto
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
            };

            var integration = await _.Client.Apps.PostIntegrationAsync(_.AppId, emailIntegrationRequest);

            Assert.True(integration.Integration.Enabled);

            await Verify(integration)
                .IgnoreMembersWithType<DateTimeOffset>();
        }
    }
}
