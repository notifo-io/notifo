﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Notifo.Infrastructure
{
    public interface ICommand<T>
    {
        bool CanCreate => false;

        Task ExecuteAsync(T target, IServiceProvider serviceProvider, CancellationToken ct);
    }
}
