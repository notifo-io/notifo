// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;

namespace Notifo.Identity.InMemory
{
    public class InMemoryApplicationStore : IOpenIddictApplicationStore<OpenIddictApplicationDescriptor>
    {
        private readonly List<OpenIddictApplicationDescriptor> applications;

        public InMemoryApplicationStore(List<OpenIddictApplicationDescriptor> applications)
        {
            this.applications = applications;
        }

        public virtual ValueTask<long> CountAsync(CancellationToken cancellationToken)
        {
            return new ValueTask<long>(applications.Count);
        }

        public virtual ValueTask<long> CountAsync<TResult>(Func<IQueryable<OpenIddictApplicationDescriptor>, IQueryable<TResult>> query, CancellationToken cancellationToken)
        {
            return new ValueTask<long>(query(applications.AsQueryable()).LongCount());
        }

        public virtual ValueTask<TResult> GetAsync<TState, TResult>(Func<IQueryable<OpenIddictApplicationDescriptor>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
        {
            var result = query(applications.AsQueryable(), state).First();

            return new ValueTask<TResult>(result);
        }

        public virtual ValueTask<OpenIddictApplicationDescriptor?> FindByClientIdAsync(string identifier, CancellationToken cancellationToken)
        {
            var result = applications.Find(x => x.ClientId == identifier);

            return new ValueTask<OpenIddictApplicationDescriptor?>(result);
        }

        public virtual ValueTask<OpenIddictApplicationDescriptor?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            var result = applications.Find(x => x.ClientId == identifier);

            return new ValueTask<OpenIddictApplicationDescriptor?>(result);
        }

        public virtual async IAsyncEnumerable<OpenIddictApplicationDescriptor> FindByPostLogoutRedirectUriAsync(string address, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var uri = new Uri(address);

            var result = applications.Where(x => x.PostLogoutRedirectUris.Contains(uri));

            foreach (var item in result)
            {
                yield return await Task.FromResult(item);
            }
        }

        public virtual async IAsyncEnumerable<OpenIddictApplicationDescriptor> FindByRedirectUriAsync(string address, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var uri = new Uri(address);

            var result = applications.Where(x => x.RedirectUris.Contains(uri));

            foreach (var item in result)
            {
                yield return await Task.FromResult(item);
            }
        }

        public virtual async IAsyncEnumerable<OpenIddictApplicationDescriptor> ListAsync(int? count, int? offset, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var result = applications;

            foreach (var item in result)
            {
                yield return await Task.FromResult(item);
            }
        }

        public virtual async IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<OpenIddictApplicationDescriptor>, TState, IQueryable<TResult>> query, TState state, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var result = query(applications.AsQueryable(), state);

            foreach (var item in result)
            {
                yield return await Task.FromResult(item);
            }
        }

        public virtual ValueTask<string?> GetIdAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken)
        {
            return new ValueTask<string?>(application.ClientId);
        }

        public virtual ValueTask<string?> GetClientIdAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken)
        {
            return new ValueTask<string?>(application.ClientId);
        }

        public virtual ValueTask<string?> GetClientSecretAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken)
        {
            return new ValueTask<string?>(application.ClientSecret);
        }

        public virtual ValueTask<string?> GetClientTypeAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken)
        {
            return new ValueTask<string?>(application.Type);
        }

        public virtual ValueTask<string?> GetConsentTypeAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken)
        {
            return new ValueTask<string?>(application.ConsentType);
        }

        public virtual ValueTask<string?> GetDisplayNameAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken)
        {
            return new ValueTask<string?>(application.DisplayName);
        }

        public virtual ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken)
        {
            return new ValueTask<ImmutableDictionary<CultureInfo, string>>(application.DisplayNames.ToImmutableDictionary());
        }

        public virtual ValueTask<ImmutableArray<string>> GetPermissionsAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken)
        {
            return new ValueTask<ImmutableArray<string>>(application.Permissions.ToImmutableArray());
        }

        public virtual ValueTask<ImmutableArray<string>> GetPostLogoutRedirectUrisAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken)
        {
            return new ValueTask<ImmutableArray<string>>(application.PostLogoutRedirectUris.Select(x => x.ToString()).ToImmutableArray());
        }

        public virtual ValueTask<ImmutableArray<string>> GetRedirectUrisAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken)
        {
            return new ValueTask<ImmutableArray<string>>(application.RedirectUris.Select(x => x.ToString()).ToImmutableArray());
        }

        public virtual ValueTask<ImmutableArray<string>> GetRequirementsAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken)
        {
            return new ValueTask<ImmutableArray<string>>(application.Requirements.ToImmutableArray());
        }

        public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken)
        {
            return new ValueTask<ImmutableDictionary<string, JsonElement>>(application.Properties.ToImmutableDictionary());
        }

        public virtual ValueTask CreateAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask UpdateAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask DeleteAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask<OpenIddictApplicationDescriptor> InstantiateAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetClientIdAsync(OpenIddictApplicationDescriptor application, string? identifier, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetClientSecretAsync(OpenIddictApplicationDescriptor application, string? secret, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetClientTypeAsync(OpenIddictApplicationDescriptor application, string? type, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetConsentTypeAsync(OpenIddictApplicationDescriptor application, string? type, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetDisplayNameAsync(OpenIddictApplicationDescriptor application, string? name, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetDisplayNamesAsync(OpenIddictApplicationDescriptor application, ImmutableDictionary<CultureInfo, string> names, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetPermissionsAsync(OpenIddictApplicationDescriptor application, ImmutableArray<string> permissions, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetPostLogoutRedirectUrisAsync(OpenIddictApplicationDescriptor application, ImmutableArray<string> addresses, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetRedirectUrisAsync(OpenIddictApplicationDescriptor application, ImmutableArray<string> addresses, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetPropertiesAsync(OpenIddictApplicationDescriptor application, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetRequirementsAsync(OpenIddictApplicationDescriptor application, ImmutableArray<string> requirements, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
