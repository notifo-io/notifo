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

public class SystemUsersTests : IClassFixture<ClientFixture>
{
    public ClientFixture _ { get; set; }

    public SystemUsersTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_lock_and_delete_system_user()
    {
        var email = $"{Guid.NewGuid()}@notifo.io";

        // STEP 0: Create system user.
        var user_0 = await _.Client.SystemUsers.PostUserAsync(new CreateSystemUserDto
        {
            Email = email,
            Password = "Secret-Password1!",
            Roles =
            [
                "ADMIN",
            ]
        });

        Assert.Equal(email, user_0.Email);
        Assert.Contains("ADMIN", user_0.Roles);
        Assert.False(user_0.IsLocked);


        // STEP 1: Query system users.
        var users = await _.Client.SystemUsers.GetUsersAsync(email);

        Assert.Contains(users.Items, x => x.Id == user_0.Id);


        // STEP 2: Update the system user.
        var emailUpdated = $"{Guid.NewGuid()}@notifo.io";

        var user_1 = await _.Client.SystemUsers.PutUserAsync(user_0.Id, new UpdateSystemUserDto
        {
            Email = emailUpdated
        });

        Assert.Equal(emailUpdated, user_1.Email);


        // STEP 3: Lock and unlock the system user.
        var user_2 = await _.Client.SystemUsers.LockUserAsync(user_0.Id);

        Assert.True(user_2.IsLocked);

        var user_3 = await _.Client.SystemUsers.UnlockUserAsync(user_0.Id);

        Assert.False(user_3.IsLocked);


        // STEP 4: Delete the system user.
        await _.Client.SystemUsers.DeleteUserAsync(user_0.Id);

        var ex = await Assert.ThrowsAsync<NotifoException>(() => _.Client.SystemUsers.GetUserAsync(user_0.Id));

        Assert.Equal(404, ex.StatusCode);
    }
}
