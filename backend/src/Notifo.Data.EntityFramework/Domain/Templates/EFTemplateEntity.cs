// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Notifo.Infrastructure;

namespace Notifo.Domain.Templates;

[Table("Templates")]
[Index(nameof(AppId))]
public sealed class EFTemplateEntity : EFEntity<Template>
{
    [MaxLength(FieldLengths.AppId)]
    public string AppId { get; set; }

    [MaxLength(FieldLengths.Key)]
    public string Code { get; set; }

    public static string CreateId(string appId, string code)
    {
        return $"{appId}_{code}";
    }

    public static EFTemplateEntity FromTemplate(Template template)
    {
        return new EFTemplateEntity
        {
            AppId = template.AppId,
            Code = template.Code,
            Doc = template,
            DocId = CreateId(template.AppId, template.Code),
            Etag = GenerateEtag(),
        };
    }

    public Template ToTemplate()
    {
        return Doc;
    }
}
