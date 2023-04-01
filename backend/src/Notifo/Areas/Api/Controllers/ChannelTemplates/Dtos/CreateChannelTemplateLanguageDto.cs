// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Areas.Api.OpenApi;
using Notifo.Domain.ChannelTemplates;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;

[OpenApiRequest]
public sealed class CreateChannelTemplateLanguageDto
{
    /// <summary>
    /// The new language.
    /// </summary>
    [Required]
    public string Language { get; set; }

    public CreateChannelTemplateLanguage<T> ToUpdate<T>(string code)
    {
        var result = SimpleMapper.Map(this, new CreateChannelTemplateLanguage<T>());

        result.TemplateCode = code;

        return result;
    }
}
