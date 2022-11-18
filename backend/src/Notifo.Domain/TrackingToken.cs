// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain;

public readonly record struct TrackingToken(Guid UserNotificationId, string? Channel = null, Guid ConfigurationId = default, string? Configuration = null)
{
    public readonly bool IsValid => UserNotificationId != default;

    public static TrackingToken Parse(string id, string? channel = null, Guid configurationId = default, string? configuration = null)
    {
        TryParse(id, channel, configurationId, configuration, out var result);

        return result;
    }

    public static bool TryParse(string id, string? channel, Guid configurationId, string? configuration, out TrackingToken result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        if (Guid.TryParse(id, out var guid))
        {
            result = new TrackingToken(guid, channel, configurationId, configuration);
            return true;
        }

        try
        {
            var decoded = id.FromBase64().Split('|');

            if (!Guid.TryParse(decoded[0], out guid))
            {
                return false;
            }

            if (decoded.Length >= 1 && !string.IsNullOrWhiteSpace(decoded[1]))
            {
                channel = decoded[1];
            }

            if (decoded.Length > 2 && Guid.TryParse(decoded[2], out var parsed) && parsed != default)
            {
                configurationId = parsed;
            }

            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = null;
            }

            result = new TrackingToken(guid, channel?.ToLowerInvariant(), configurationId, configuration);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public readonly string ToParsableString()
    {
        var channel = Channel?.ToLowerInvariant();

        var compound =
            ConfigurationId == default ?
            $"{UserNotificationId}|{channel}" :
            $"{UserNotificationId}|{channel}|{ConfigurationId}";

        return compound.ToBase64();
    }
}
