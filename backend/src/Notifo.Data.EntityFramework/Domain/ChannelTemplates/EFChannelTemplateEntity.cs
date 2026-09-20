// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Notifo.Infrastructure;

namespace Notifo.Domain.ChannelTemplates;

[Table("ChannelTemplates")]
[Index(nameof(AppId), nameof(Type))]
public sealed class EFChannelTemplateEntity : EFEntity<string>
{
    [MaxLength(FieldLengths.AppId)]
    public string AppId { get; set; }

    [MaxLength(FieldLengths.AppId)]
    public string Type { get; set; }

    [MaxLength(FieldLengths.Text)]
    public string? Name { get; set; }

    public bool Primary { get; set; }

    public static string CreateId<T>(string appId, string id)
    {
        return $"{typeof(T).Name}_{appId}_{id}";
    }

    public static EFChannelTemplateEntity FromChannelTemplate<T>(ChannelTemplate<T> template, JsonSerializerOptions jsonOptions)
    {
        return new EFChannelTemplateEntity
        {
            Name = template.Name.ToMaxLength(FieldLengths.Text),
            AppId = template.AppId,
            Doc = JsonSerializer.Serialize(template, jsonOptions),
            DocId = CreateId<T>(template.AppId, template.Id),
            Etag = GenerateEtag(),
            Primary = template.Primary,
            Type = typeof(T).Name,
        };
    }

    public ChannelTemplate<T> ToChannelTemplate<T>(JsonSerializerOptions jsonOptions)
    {
        return JsonSerializer.Deserialize<ChannelTemplate<T>>(Doc, jsonOptions)!;
    }
}
