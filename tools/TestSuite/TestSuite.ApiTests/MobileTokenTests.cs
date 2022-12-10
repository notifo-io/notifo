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

public class MobileTokenTests : IClassFixture<CreatedAppFixture>
{
    private readonly string token = Guid.NewGuid().ToString();

    public CreatedAppFixture _ { get; set; }

    public MobileTokenTests(CreatedAppFixture fixture)
    {
        VerifierSettings.DontScrubDateTimes();

        _ = fixture;
    }

    [Fact]
    public async Task Should_create_and_fetch_token_as_user()
    {
        // STEP 1: Create user.
        var user = await CreateUserAsync();

        var client = _.BuildUserClient(user);


        // STEP 2: Create token
        await client.MobilePush.PostMyTokenAsync(new RegisterMobileTokenDto { Token = token });


        // STEP 3: Get tokens.
        var tokens_0 = await client.MobilePush.GetMyTokenAsync();

        Assert.Contains(tokens_0.Items, x => x.Token == token);


        // STEP 4: Delete token
        await client.MobilePush.DeleteMyTokenAsync(token);

        var tokens_1 = await client.MobilePush.GetMyTokenAsync();

        Assert.DoesNotContain(tokens_1.Items, x => x.Token == token);
    }

    private async Task<UserDto> CreateUserAsync()
    {
        var userRequest = new UpsertUsersDto
        {
            Requests = new List<UpsertUserDto>
            {
                new UpsertUserDto()
            }
        };

        var users_0 = await _.Client.Users.PostUsersAsync(_.AppId, userRequest);
        var user_0 = users_0.First();

        return user_0;
    }
}
