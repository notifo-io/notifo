// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Notifo.Infrastructure;

namespace Notifo.Identity;

[Table("Identity_Xml")]
public sealed class EFXmlEntity
{
    [Key]
    [MaxLength(FieldLengths.Key)]
    public string FriendlyName { get; set; }

    public string Xml { get; set; }
}
