// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain;

public sealed class NotificationProperties : Dictionary<string, string>
{
    public NotificationProperties()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    public NotificationProperties(IReadOnlyDictionary<string, string> source)
        : base(source, StringComparer.OrdinalIgnoreCase)
    {
    }
}
