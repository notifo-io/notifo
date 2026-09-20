// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Id should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class UsersTests : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; set; }

    public UsersTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Theory]
    [InlineData(ClientMode.ClientId)]
    [InlineData(ClientMode.ApiKey)]
    public async Task Should_create_users(ClientMode mode)
    {
        // STEP 0: Create user.
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();

        var userRequest = new UpsertUsersDto
        {
            Requests =
            [
                new UpsertUserDto
                {
                    Id = userId1,
                    FullName = "name1_0"
                },
                new UpsertUserDto
                {
                    Id = userId2,
                    FullName = "name2_0"
                },
            ]
        };

        var users = await _.GetClient(mode).Users.PostUsersAsync(_.AppId, userRequest);

        Assert.Equal(2, users.Count);

        var user1 = users.ElementAt(0);
        var user2 = users.ElementAt(1);

        Assert.Equal(userId1, user1.Id);
        Assert.Equal(userId2, user2.Id);
        Assert.Equal("name1_0", user1.FullName);
        Assert.Equal("name2_0", user2.FullName);

        await Verify(user1)
            .UseParameters(mode)
            .IgnoreMembersWithType<DateTimeOffset>()
            .IgnoreMember<UserDto>(x => x.ApiKey);
    }

    [Theory]
    [InlineData(ClientMode.ClientId)]
    [InlineData(ClientMode.ApiKey)]
    public async Task Should_find_user(ClientMode mode)
    {
        // STEP 0: Create user.
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();

        var userRequest = new UpsertUsersDto
        {
            Requests =
            [
                new UpsertUserDto
                {
                    Id = userId1,
                    FullName = userId1
                },
                new UpsertUserDto
                {
                    Id = userId2,
                    FullName = userId2
                },
            ]
        };

        await _.GetClient(mode).Users.PostUsersAsync(_.AppId, userRequest);


        // STEP 1: Query users
        var users = await _.GetClient(mode).Users.GetUsersAsync(_.AppId, userId1);

        Assert.Equal(1, users.Total);
        Assert.Equal(userId1, users.Items[0].Id);

        await Verify(users)
            .UseParameters(mode)
            .IgnoreMembersWithType<DateTimeOffset>()
            .IgnoreMember<UserDto>(x => x.ApiKey);
    }

    [Theory]
    [InlineData(ClientMode.ClientId)]
    [InlineData(ClientMode.ApiKey)]
    public async Task Should_update_users(ClientMode mode)
    {
        // STEP 0: Create user.
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();

        var userRequest = new UpsertUsersDto
        {
            Requests =
            [
                new UpsertUserDto
                {
                    Id = userId1,
                    FullName = "name1_0"
                },
                new UpsertUserDto
                {
                    Id = userId2,
                    FullName = "name2_0"
                },
            ]
        };

        await _.GetClient(mode).Users.PostUsersAsync(_.AppId, userRequest);


        // STEP 1: Update user.
        var userRequest2 = new UpsertUsersDto
        {
            Requests =
            [
                new UpsertUserDto
                {
                    Id = userId1,
                    FullName = "name1_1"
                },
            ]
        };

        await _.GetClient(mode).Users.PostUsersAsync(_.AppId, userRequest2);


        // Get users
        var users = await _.GetClient(mode).Users.GetUsersAsync(_.AppId, take: 100000);

        var user1 = users.Items.SingleOrDefault(x => x.Id == userId1);
        var user2 = users.Items.SingleOrDefault(x => x.Id == userId2);

        Assert.Equal("name1_1", user1?.FullName);
        Assert.Equal("name2_0", user2?.FullName);

        await Verify(user1)
            .UseParameters(mode)
            .IgnoreMembersWithType<DateTimeOffset>()
            .IgnoreMember<UserDto>(x => x.ApiKey);
    }

    [Theory]
    [InlineData(ClientMode.ClientId)]
    [InlineData(ClientMode.ApiKey)]
    public async Task Should_delete_users(ClientMode mode)
    {
        // STEP 0: Create user.
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();

        var userRequest = new UpsertUsersDto
        {
            Requests =
            [
                new UpsertUserDto
                {
                    Id = userId1,
                    FullName = "name1_0"
                },
                new UpsertUserDto
                {
                    Id = userId2,
                    FullName = "name2_0"
                },
            ]
        };

        await _.GetClient(mode).Users.PostUsersAsync(_.AppId, userRequest);


        // STEP 1: Delete user.
        await _.GetClient(mode).Users.DeleteUserAsync(_.AppId, userId1);


        // Get users
        var users = await _.GetClient(mode).Users.GetUsersAsync(_.AppId, take: 100000);

        var user1 = users.Items.SingleOrDefault(x => x.Id == userId1);
        var user2 = users.Items.SingleOrDefault(x => x.Id == userId2);

        Assert.Null(user1);
        Assert.NotNull(user2);
    }

    [Fact]
    public async Task Should_add_and_remove_allowed_topic()
    {
        var topicPrefix = $"news/{Guid.NewGuid()}";

        // STEP 0: Create a user that can only subscribe to whitelisted topics.
        var user_0 = await _.CreateUserAsync(x => x.RequiresWhitelistedTopics = true);

        var subscribeRequest = new SubscribeManyDto
        {
            Subscribe =
            [
                new SubscribeDto { TopicPrefix = $"{topicPrefix}/daily" },
            ]
        };


        // STEP 1: Subscribing is rejected while the topic is not whitelisted.
        var ex_0 = await Assert.ThrowsAsync<NotifoException>(() => _.Client.Users.PostSubscriptionsAsync(_.AppId, user_0.Id, subscribeRequest));

        Assert.Equal(403, ex_0.StatusCode);


        // STEP 2: Allow the topic.
        await _.Client.Users.PostAllowedTopicAsync(_.AppId, user_0.Id, new AddAllowedTopicDto { Prefix = topicPrefix });

        var profile_0 = await _.BuildUserClient(user_0).User.GetUserAsync();

        Assert.Contains(topicPrefix, profile_0.AllowedTopics);


        // STEP 3: Subscribing works now.
        await _.Client.Users.PostSubscriptionsAsync(_.AppId, user_0.Id, subscribeRequest);

        var subscriptions = await _.Client.Users.GetSubscriptionsAsync(_.AppId, user_0.Id, take: 100000);

        Assert.Contains(subscriptions.Items, x => x.TopicPrefix == $"{topicPrefix}/daily");


        // STEP 4: Remove the allowed topic again.
        await _.Client.Users.DeleteAllowedTopicAsync(_.AppId, user_0.Id, topicPrefix);

        var profile_1 = await _.BuildUserClient(user_0).User.GetUserAsync();

        Assert.DoesNotContain(topicPrefix, profile_1.AllowedTopics);

        var ex_1 = await Assert.ThrowsAsync<NotifoException>(() => _.Client.Users.PostSubscriptionsAsync(_.AppId, user_0.Id, subscribeRequest));

        Assert.Equal(403, ex_1.StatusCode);
    }

    [Fact]
    public async Task Should_get_user_with_details()
    {
        var token = Guid.NewGuid().ToString();

        // STEP 0: Create user.
        var user_0 = await _.CreateUserAsync(x =>
        {
            x.EmailAddress = "user@notifo.io";
            x.FullName = "John Doe";
            x.PhoneNumber = "+491234567890";
            x.PreferredLanguage = "de";
            x.PreferredTimezone = "Europe/Berlin";
            x.Properties = new Dictionary<string, string>
            {
                ["custom"] = "value"
            };
        });


        // STEP 1: Register a mobile push token to get a detail that is loaded separately.
        await _.BuildUserClient(user_0).MobilePush.PostMyTokenAsync(new RegisterMobileTokenDto
        {
            Token = token,
            DeviceType = MobileDeviceType.Android
        });


        // STEP 2: Get the user with details.
        var user_1 = await _.Client.Users.GetUserAsync(_.AppId, user_0.Id, true);

        Assert.Equal("user@notifo.io", user_1.EmailAddress);
        Assert.Equal("John Doe", user_1.FullName);
        Assert.Equal("+491234567890", user_1.PhoneNumber);
        Assert.Equal("de", user_1.PreferredLanguage);
        Assert.Equal("Europe/Berlin", user_1.PreferredTimezone);
        Assert.Equal("value", user_1.Properties["custom"]);
        Assert.Contains(user_1.MobilePushTokens, x => x.Token == token);
        Assert.NotNull(user_1.Counters);
    }

    [Fact]
    public async Task Should_update_own_profile()
    {
        // STEP 0: Create user.
        var user_0 = await _.CreateUserAsync();

        var client = _.BuildUserClient(user_0);


        // STEP 1: Update the profile.
        var updateRequest = new UpdateProfileDto
        {
            EmailAddress = "profile@notifo.io",
            FullName = "Jane Doe",
            PreferredLanguage = "de",
            PreferredTimezone = "Europe/Berlin",
            Settings = new Dictionary<string, ChannelSettingDto>
            {
                [Providers.Email] = new ChannelSettingDto
                {
                    Send = ChannelSend.Send
                }
            }
        };

        var profile_0 = await client.User.PostUserAsync(updateRequest);

        Assert.Equal("profile@notifo.io", profile_0.EmailAddress);
        Assert.Equal("Jane Doe", profile_0.FullName);
        Assert.Equal("de", profile_0.PreferredLanguage);


        // STEP 2: Read the profile back.
        var profile_1 = await client.User.GetUserAsync();

        Assert.Equal("profile@notifo.io", profile_1.EmailAddress);
        Assert.Equal("Jane Doe", profile_1.FullName);
        Assert.Equal("de", profile_1.PreferredLanguage);
        Assert.Equal("Europe/Berlin", profile_1.PreferredTimezone);
        Assert.Equal(ChannelSend.Send, profile_1.Settings[Providers.Email].Send);


        // STEP 3: The admin endpoint returns the same values.
        var user_1 = await _.Client.Users.GetUserAsync(_.AppId, user_0.Id);

        Assert.Equal("profile@notifo.io", user_1.EmailAddress);
        Assert.Equal("Jane Doe", user_1.FullName);
    }
}
