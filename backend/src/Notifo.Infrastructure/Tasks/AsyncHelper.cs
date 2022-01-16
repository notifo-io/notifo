// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Tasks
{
    public static class AsyncHelper
    {
        public static async Task<(T1, T2)> WhenAll<T1, T2>(Task<T1> task1, Task<T2> task2)
        {
            await Task.WhenAll(task1, task2);

#pragma warning disable MA0042 // Do not use blocking calls in an async method
            return (task1.Result, task2.Result);
#pragma warning restore MA0042 // Do not use blocking calls in an async method
        }

        public static async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(Task<T1> task1, Task<T2> task2, Task<T3> task3)
        {
            await Task.WhenAll(task1, task2, task3);

#pragma warning disable MA0042 // Do not use blocking calls in an async method
            return (task1.Result, task2.Result, task3.Result);
#pragma warning restore MA0042 // Do not use blocking calls in an async method
        }
    }
}
