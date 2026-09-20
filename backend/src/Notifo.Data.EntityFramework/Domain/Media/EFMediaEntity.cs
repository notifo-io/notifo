// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Notifo.Infrastructure;

namespace Notifo.Domain.Media;

[Table("Media")]
[Index(nameof(AppId), nameof(LastUpdate))]
public sealed class EFMediaEntity : EFEntity<Media>
{
    [MaxLength(FieldLengths.AppId)]
    public string AppId { get; set; }

    [MaxLength(FieldLengths.Key)]
    public string FileName { get; set; }

    public Instant LastUpdate { get; set; }

    public static string CreateId(string appId, string fileName)
    {
        return $"{appId}_{fileName}";
    }

    public static EFMediaEntity FromMedia(Media media)
    {
        return new EFMediaEntity
        {
            AppId = media.AppId,
            Doc = media,
            DocId = CreateId(media.AppId, media.FileName),
            Etag = GenerateEtag(),
            FileName = media.FileName,
            LastUpdate = media.LastUpdate,
        };
    }

    public Media ToMedia()
    {
        return Doc;
    }
}
