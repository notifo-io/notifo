// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Data.Common;

namespace Notifo.Infrastructure.Migrations;

public abstract class ConnectionStringParser
{
    public string? GetHostName(string? source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return null;
        }

        try
        {
            return GetProviderSpecificHostName(source);
        }
        catch
        {
            return GetFallbackHostName(source);
        }
    }

    private static string? GetFallbackHostName(string source)
    {
        try
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = source,
            };

            if (builder.TryGetValue("Server", out var server))
            {
                return server?.ToString();
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    protected abstract string? GetProviderSpecificHostName(string source);
}
