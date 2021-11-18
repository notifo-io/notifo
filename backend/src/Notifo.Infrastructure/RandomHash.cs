// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Cryptography;
using System.Text;

namespace Notifo.Infrastructure
{
    public static class RandomHash
    {
        public static string New()
        {
            return Guid.NewGuid()
                .ToString().Sha256Base64()
                .ToLowerInvariant()
                .Replace("+", "x", StringComparison.OrdinalIgnoreCase)
                .Replace("=", "x", StringComparison.OrdinalIgnoreCase)
                .Replace("/", "x", StringComparison.OrdinalIgnoreCase);
        }

        public static string Simple()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        public static string Sha256Base64(this string value)
        {
            return Sha256Base64(Encoding.UTF8.GetBytes(value));
        }

        public static string Sha256Base64(this byte[] bytes)
        {
            using (var sha = SHA256.Create())
            {
                var bytesHash = sha.ComputeHash(bytes);

                var result = Convert.ToBase64String(bytesHash);

                return result;
            }
        }
    }
}
