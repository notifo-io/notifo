// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations.Seven.Implementation;

public interface ISevenSmsClient
{
    Task<SevenSmsResponse> SendSmsAsync(string to, string text, string? from,
        CancellationToken ct);
}
