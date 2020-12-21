// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Log;

namespace Notifo.Domain.Utils
{
    public static class LogExtensions
    {
        public static async Task ProfileAsync(this ISemanticLog log, string actionName, Func<Task> action)
        {
            using (Profiler.StartSession())
            {
                try
                {
                    await action();

                    log.LogInformation(actionName, (c, w) =>
                    {
                        w.WriteProperty("action", c);
                        w.WriteProperty("status", "Completed");

                        Profiler.Session?.Write(w);
                    });
                }
                catch (Exception ex)
                {
                    log.LogWarning(ex, actionName, (c, w) =>
                    {
                        w.WriteProperty("action", c);
                        w.WriteProperty("status", "Failed");

                        Profiler.Session?.Write(w);
                    });

                    throw;
                }
            }
        }
    }
}
