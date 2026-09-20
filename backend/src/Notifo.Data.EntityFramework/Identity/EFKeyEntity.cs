// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace Notifo.Identity;

[Table("Identity_Keys")]
public sealed class EFKeyEntity
{
    [Key]
    [MaxLength(100)]
    public string Id { get; set; }

    [MaxLength(100)]
    public string KeyId { get; set; }

    public byte[]? D { get; set; }

    public byte[]? DP { get; set; }

    public byte[]? DQ { get; set; }

    public byte[]? Exponent { get; set; }

    public byte[]? InverseQ { get; set; }

    public byte[]? Modulus { get; set; }

    public byte[]? P { get; set; }

    public byte[]? Q { get; set; }

    public static EFKeyEntity Create(string id, string keyId, RSAParameters source)
    {
        return new EFKeyEntity
        {
            D = source.D,
            DP = source.DP,
            DQ = source.DQ,
            Exponent = source.Exponent,
            Id = id,
            InverseQ = source.InverseQ,
            KeyId = keyId,
            Modulus = source.Modulus,
            P = source.P,
            Q = source.Q,
        };
    }

    public RSAParameters ToParameters()
    {
        return new RSAParameters
        {
            D = D,
            DP = DP,
            DQ = DQ,
            Exponent = Exponent,
            InverseQ = InverseQ,
            Modulus = Modulus,
            P = P,
            Q = Q,
        };
    }
}
