// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;

namespace Notifo.Pipeline;

public static class VersionProvider
{
    public static string Current
    {
        get
        {
            var version = Environment.GetEnvironmentVariable("NOTIFO__RUNTIME__VERSION");

            if (string.IsNullOrWhiteSpace(version))
            {
                version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                version = "1.0";
            }

            return version;
        }
    }
}
