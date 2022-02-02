using FakeItEasy;
using Notifo.Domain.Channels.MobilePush;
using Notifo.Infrastructure.Collections;
using Xunit;

namespace Notifo.Domain.Users
{
    public class RemoveUserMobileTokenTests
    {
        [Fact]
        public async Task Should_remove_token_if_token_exists()
        {
            var token1 = "test token 1";
            var token2 = "test token 2";
            var token3 = "test token 3";

            var sut = new RemoveUserMobileToken
            {
                Token = token1
            };

            var user = new User
            {
                MobilePushTokens = new List<string>
                    {
                        token1,
                        token2,
                        token3
                    }
                    .Select(t => new MobilePushToken { Token = t })
                    .ToReadonlyList(),
            };

            var updatedUser = await sut.ExecuteAsync(user, A.Fake<IServiceProvider>(), CancellationToken.None);

            Assert.Equal(new[]
            {
                token2,
                token3
            }, updatedUser!.MobilePushTokens.Select(x => x.Token).OrderBy(x => x).ToArray());
        }

        [Fact]
        public async Task Should_not_change_tokens_if_token_not_exists()
        {
            var token1 = "test token 1";
            var token2 = "test token 2";
            var token3 = "test token 3";

            var sut = new RemoveUserMobileToken
            {
                Token = token1
            };

            var user = new User
            {
                MobilePushTokens = new List<string>
                    {
                        token2,
                        token3
                    }
                    .Select(t => new MobilePushToken { Token = t })
                    .ToReadonlyList(),
            };

            var updatedUser = await sut.ExecuteAsync(user, A.Fake<IServiceProvider>(), CancellationToken.None);

            Assert.Null(updatedUser);
        }

        [Fact]
        public async Task Should_remove_single_token()
        {
            var token1 = "test token 1";

            var sut = new RemoveUserMobileToken
            {
                Token = token1
            };

            var user = new User
            {
                MobilePushTokens = new List<string>
                    {
                        token1
                    }
                    .Select(t => new MobilePushToken { Token = t })
                    .ToReadonlyList(),
            };

            var updatedUser = await sut.ExecuteAsync(user, A.Fake<IServiceProvider>(), CancellationToken.None);

            Assert.Empty(updatedUser!.MobilePushTokens);
        }
    }
}
