// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Notifo.Infrastructure;

public abstract class EFEntity<T> where T : class
{
    [Key]
    [MaxLength(FieldLengths.Key)]
    public string DocId { get; set; }

    [MaxLength(FieldLengths.Etag)]
    public string Etag { get; set; }

    [Json]
    public T Doc { get; set; }

    public static string GenerateEtag()
    {
        return Guid.NewGuid().ToString("N");
    }
}
