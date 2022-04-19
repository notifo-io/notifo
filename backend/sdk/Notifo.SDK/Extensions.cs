// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;

namespace Notifo.SDK
{
    internal static class Extensions
    {
        public static void Configure(this JsonSerializerSettings settings)
        {
            settings.Converters.Add(DateTimeOffsetConverter.Instance);
        }
    }
}
