// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notifo.Infrastructure.KeyValueStore;

[Table("KeyValueStore")]
public sealed class EFKeyValueEntity
{
    [Key]
    [MaxLength(FieldLengths.Key)]
    public string Key { get; set; }

    [MaxLength(FieldLengths.LongText)]
    public string? Value { get; set; }
}
