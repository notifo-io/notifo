// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;

namespace Notifo.Domain.Integrations.MessageBird.Implementation;

public interface IMessageBirdClient
{
    Task<SmsWebhookRequest> ParseSmsWebhookAsync(HttpContext httpContext);

    Task<WhatsAppWebhookRequest> ParseWhatsAppWebhookAsync(HttpContext httpContext);

    Task<SmsResponse> SendSmsAsync(SmsMessage message,
        CancellationToken ct);

    Task<ConversationResponse> SendWhatsAppAsync(WhatsAppTemplateMessage message,
        CancellationToken ct);

    Task<ConversationResponse> SendWhatsAppAsync(WhatsAppTextMessage message,
        CancellationToken ct);

    Task<ConversationResponse> GetMessageAsync(string id,
        CancellationToken ct);
}
