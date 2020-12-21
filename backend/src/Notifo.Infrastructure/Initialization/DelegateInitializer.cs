// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Notifo.Infrastructure.Initialization
{
    public sealed class DelegateInitializer : IInitializable
    {
        private readonly string name;
        private readonly Func<CancellationToken, Task> action;

        public DelegateInitializer(string name, Func<CancellationToken, Task> action)
        {
            Guard.NotNull(action, nameof(action));

            this.name = name;

            this.action = action;
        }

        public override string ToString()
        {
            return name;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            return action(ct);
        }
    }
}
