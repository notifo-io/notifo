// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using Notifo.Areas.Api.Controllers.Apps.Dtos;
using Notifo.Areas.Api.Controllers.Events.Dtos;

namespace Notifo.Areas.Api
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(AddContributorDto))]
    [JsonSerializable(typeof(AppContributorDto))]
    [JsonSerializable(typeof(AppDetailsDto))]
    [JsonSerializable(typeof(EventDto))]
    public partial class AppJsonContext : JsonSerializerContext
    {
        static void Foo()
        {
            var c = new AppJsonContext();
            // c._A
            // c._EventDto
        }
    }
}
