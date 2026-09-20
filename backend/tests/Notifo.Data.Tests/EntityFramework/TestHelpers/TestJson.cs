// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain;
using Notifo.Domain.Utils;

namespace Notifo.EntityFramework.TestHelpers;

public static class TestJson
{
    // Same configuration as in the host.
    public static readonly JsonSerializerOptions Options =
        new JsonSerializerOptions().Configure(options =>
        {
            options.Converters.Add(new JsonSoftEnumConverter<ConfirmMode>());
            options.Converters.Add(new JsonTopicIdConverter());
        });
}
