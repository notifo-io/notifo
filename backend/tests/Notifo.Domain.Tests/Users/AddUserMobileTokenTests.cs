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

            string token1 = "test token 1";
            string token2 = "test token 2";
            string token3 = "test token 3";

            sut.Token = new MobilePushToken { Token = token1 };

            var user = new User
            {
                MobilePushTokens = new List<string> { token2, token3 }
                        .Select(t => new MobilePushToken { Token = t })
                        .ToReadonlyList(),
            };

            var updatedUser = await sut.ExecuteAsync(user, A.Fake<IServiceProvider>(), CancellationToken.None);

            Assert.Contains(updatedUser?.MobilePushTokens, t => t.Token == token1);
            Assert.Contains(updatedUser?.MobilePushTokens, t => t.Token == token2);
            Assert.Contains(updatedUser?.MobilePushTokens, t => t.Token == token3);
        }
    }
}
