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
    public class InMemoryScopeStore : IOpenIddictScopeStore<OpenIddictScopeDescriptor>
    {
        private readonly List<OpenIddictScopeDescriptor> scopes;

        public InMemoryScopeStore(params OpenIddictScopeDescriptor[] scopes)
        {
            this.scopes = scopes.ToList();
        }

        public InMemoryScopeStore(List<OpenIddictScopeDescriptor> scopes)
        {
            this.scopes = scopes;
        }

        public virtual ValueTask<long> CountAsync(CancellationToken cancellationToken)
        {
            return new ValueTask<long>(scopes.Count);
        }

        public virtual ValueTask<long> CountAsync<TResult>(Func<IQueryable<OpenIddictScopeDescriptor>, IQueryable<TResult>> query, CancellationToken cancellationToken)
        {
            return new ValueTask<long>(query(scopes.AsQueryable()).LongCount());
        }

        public virtual ValueTask<TResult> GetAsync<TState, TResult>(Func<IQueryable<OpenIddictScopeDescriptor>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
        {
            var result = query(scopes.AsQueryable(), state).First();

            return new ValueTask<TResult>(result);
        }

        public virtual ValueTask<OpenIddictScopeDescriptor?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            var result = scopes.Find(x => x.Name == identifier);

            return new ValueTask<OpenIddictScopeDescriptor?>(result);
        }

        public virtual ValueTask<OpenIddictScopeDescriptor?> FindByNameAsync(string name, CancellationToken cancellationToken)
        {
            var result = scopes.Find(x => x.Name == name);

            return new ValueTask<OpenIddictScopeDescriptor?>(result);
        }

        public virtual async IAsyncEnumerable<OpenIddictScopeDescriptor> FindByNamesAsync(ImmutableArray<string> names, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var result = scopes.Where(x => x.Name != null && names.Contains(x.Name));

            foreach (var item in result)
            {
                yield return await Task.FromResult(item);
            }
        }

        public virtual async IAsyncEnumerable<OpenIddictScopeDescriptor> FindByResourceAsync(string resource, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var result = scopes.Where(x => x.Resources.Contains(resource));

            foreach (var item in result)
            {
                yield return await Task.FromResult(item);
            }
        }

        public virtual async IAsyncEnumerable<OpenIddictScopeDescriptor> ListAsync(int? count, int? offset, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var result = scopes;

            foreach (var item in result)
            {
                yield return await Task.FromResult(item);
            }
        }

        public virtual async IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<OpenIddictScopeDescriptor>, TState, IQueryable<TResult>> query, TState state, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var result = query(scopes.AsQueryable(), state);

            foreach (var item in result)
            {
                yield return await Task.FromResult(item);
            }
        }

        public virtual ValueTask<string?> GetIdAsync(OpenIddictScopeDescriptor scope, CancellationToken cancellationToken)
        {
            return new ValueTask<string?>(scope.Name);
        }

        public virtual ValueTask<string?> GetNameAsync(OpenIddictScopeDescriptor scope, CancellationToken cancellationToken)
        {
            return new ValueTask<string?>(scope.Name);
        }

        public virtual ValueTask<string?> GetDescriptionAsync(OpenIddictScopeDescriptor scope, CancellationToken cancellationToken)
        {
            return new ValueTask<string?>(scope.Description);
        }

        public virtual ValueTask<string?> GetDisplayNameAsync(OpenIddictScopeDescriptor scope, CancellationToken cancellationToken)
        {
            return new ValueTask<string?>(scope.DisplayName);
        }

        public virtual ValueTask<ImmutableDictionary<CultureInfo, string>> GetDescriptionsAsync(OpenIddictScopeDescriptor scope, CancellationToken cancellationToken)
        {
            return new ValueTask<ImmutableDictionary<CultureInfo, string>>(scope.Descriptions.ToImmutableDictionary());
        }

        public virtual ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(OpenIddictScopeDescriptor scope, CancellationToken cancellationToken)
        {
            return new ValueTask<ImmutableDictionary<CultureInfo, string>>(scope.DisplayNames.ToImmutableDictionary());
        }

        public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(OpenIddictScopeDescriptor scope, CancellationToken cancellationToken)
        {
            return new ValueTask<ImmutableDictionary<string, JsonElement>>(scope.Properties.ToImmutableDictionary());
        }

        public virtual ValueTask<ImmutableArray<string>> GetResourcesAsync(OpenIddictScopeDescriptor scope, CancellationToken cancellationToken)
        {
            return new ValueTask<ImmutableArray<string>>(scope.Resources.ToImmutableArray());
        }

        public virtual ValueTask CreateAsync(OpenIddictScopeDescriptor scope, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask UpdateAsync(OpenIddictScopeDescriptor scope, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask DeleteAsync(OpenIddictScopeDescriptor scope, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask<OpenIddictScopeDescriptor> InstantiateAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetDescriptionAsync(OpenIddictScopeDescriptor scope, string? description, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetDescriptionsAsync(OpenIddictScopeDescriptor scope, ImmutableDictionary<CultureInfo, string> descriptions, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetDisplayNameAsync(OpenIddictScopeDescriptor scope, string? name, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetDisplayNamesAsync(OpenIddictScopeDescriptor scope, ImmutableDictionary<CultureInfo, string> names, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetNameAsync(OpenIddictScopeDescriptor scope, string? name, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetPropertiesAsync(OpenIddictScopeDescriptor scope, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetResourcesAsync(OpenIddictScopeDescriptor scope, ImmutableArray<string> resources, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
