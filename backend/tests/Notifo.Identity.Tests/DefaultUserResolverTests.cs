// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Identity;
using Notifo.Infrastructure;
using Xunit;

namespace Notifo.Identity;

public class DefaultUserResolverTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IUserService userService = A.Fake<IUserService>();
    private readonly DefaultUserResolver sut;

    public DefaultUserResolverTests()
    {
        ct = cts.Token;

        var serviceProvider = A.Fake<IServiceProvider>();

        var scope = A.Fake<IServiceScope>();

        var scopeFactory = A.Fake<IServiceScopeFactory>();

        A.CallTo(() => scopeFactory.CreateScope())
            .Returns(scope);

        A.CallTo(() => scope.ServiceProvider)
            .Returns(serviceProvider);

        A.CallTo(() => serviceProvider.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactory);

        A.CallTo(() => serviceProvider.GetService(typeof(IUserService)))
            .Returns(userService);

        sut = new DefaultUserResolver(serviceProvider);
    }

    [Fact]
    public async Task Should_create_user_and_return_true_when_created()
    {
        var email = "123@email.com";

        var user = A.Fake<IUser>();

        A.CallTo(() => userService.CreateAsync(email, A<UserValues>.That.Matches(x => x.Invited == true), false, ct))
            .Returns(user);

        A.CallTo(() => userService.FindByEmailAsync(email, ct))
            .Returns((IUser?)null);

        var result = await sut.CreateUserIfNotExistsAsync(email, true, ct);

        Assert.Equal((user, true), result);
    }

    [Fact]
    public async Task Should_create_user_and_return_false_when_exception_thrown()
    {
        var email = "123@email.com";

        var user = A.Fake<IUser>();

        A.CallTo(() => userService.CreateAsync(email, A<UserValues>._, false, ct))
            .Throws(new InvalidOperationException());

        A.CallTo(() => userService.FindByEmailAsync(email, ct))
            .Returns(user);

        var result = await sut.CreateUserIfNotExistsAsync(email, true, ct);

        Assert.Equal((user, false), result);
    }

    [Fact]
    public async Task Should_resolve_user_by_email()
    {
        var id = "123@email.com";

        var user = A.Fake<IUser>();

        A.CallTo(() => userService.FindByEmailAsync(id, ct))
            .Returns(user);

        var result = await sut.FindByIdOrEmailAsync(id, ct);

        Assert.Equal(user, result);
    }

    [Fact]
    public async Task Should_resolve_user_by_id()
    {
        var id = "123";

        var user = A.Fake<IUser>();

        A.CallTo(() => userService.FindByIdAsync(id, ct))
            .Returns(user);

        var result = await sut.FindByIdOrEmailAsync(id, ct);

        Assert.Equal(user, result);
    }

    [Fact]
    public async Task Should_resolve_user_by_id_only()
    {
        var id = "123";

        var user = A.Fake<IUser>();

        A.CallTo(() => userService.FindByIdAsync(id, ct))
            .Returns(user);

        var result = await sut.FindByIdOrEmailAsync(id, ct);

        Assert.Equal(user, result);
    }

    [Fact]
    public async Task Should_query_many_by_email()
    {
        var email = "hello@squidex.io";

        var users = ResultList.CreateFrom(0, A.Fake<IUser>());

        A.CallTo(() => userService.QueryAsync(email, 10, 0, ct))
            .Returns(users);

        var result = await sut.QueryByEmailAsync(email, ct);

        Assert.Single(result);
    }

    [Fact]
    public async Task Should_query_by_ids()
    {
        var ids = new[] { "1", "2" };

        var users = ResultList.CreateFrom(0, A.Fake<IUser>());

        A.CallTo(() => userService.QueryAsync(ids, ct))
            .Returns(users);

        var result = await sut.QueryManyAsync(ids, ct);

        Assert.Single(result);
    }

    [Fact]
    public async Task Should_query_all()
    {
        var users = ResultList.CreateFrom(0, A.Fake<IUser>());

        A.CallTo(() => userService.StreamAsync(ct))
            .Returns(users.ToAsyncEnumerable());

        var result = await sut.QueryAllAsync(ct).ToListAsync(ct);

        Assert.Single(result);
    }
}
