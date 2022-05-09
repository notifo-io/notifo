// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Notifo.Domain.Channels.MobilePush;
using Notifo.Infrastructure.Collections;
using Xunit;

namespace Notifo.Domain.Users
{
    public class AddUserMobileTokenTests
    {
        [Fact]
        public async Task Should_not_remove_existing_tokens_when_new_token_added()
        {
            var sut = new AddUserMobileToken();

            var token1 = "test token 1";
            var token2 = "test token 2";
            var token3 = "test token 3";

            sut.Token = new MobilePushToken { Token = token1 };

            var user = new User("1", "1", default)
            {
                MobilePushTokens = new List<string>
                    {
                        token2,
                        token3
                    }
                    .Select(t => new MobilePushToken { Token = t })
                    .ToReadonlyList(),
            };

            var updatedUser = await sut.ExecuteAsync(user, A.Fake<IServiceProvider>(), default);

            Assert.Equal(new[]
            {
                token1,
                token2,
                token3
            }, updatedUser!.MobilePushTokens.Select(x => x.Token).OrderBy(x => x).ToArray());
        }

        [Fact]
        public async Task Should_not_change_existing_token_if_token_added_again()
        {
            var sut = new AddUserMobileToken();

            var token1 = "test token 1";
            var token2 = "test token 2";

            sut.Token = new MobilePushToken { Token = token1 };

            var user = new User("1", "1", default)
            {
                MobilePushTokens = new List<string>
                    {
                        token1,
                        token2
                    }
                    .Select(t => new MobilePushToken { Token = t })
                    .ToReadonlyList(),
            };

            var updatedUser = await sut.ExecuteAsync(user, A.Fake<IServiceProvider>(), default);

            Assert.Null(updatedUser);
        }
    }
}
