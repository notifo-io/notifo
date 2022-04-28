// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Identity;
using Notifo.Domain.Users;
using Notifo.Pipeline;
using NSwag.Annotations;
using IEmailTemplateStore = Notifo.Domain.ChannelTemplates.IChannelTemplateStore<Notifo.Domain.Channels.Email.EmailTemplate>;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Notifo.Areas.Api.Controllers.ChannelTemplates
{
    [OpenApiTag("EmailTemplates")]
    public class EmailTemplatePreviewController : BaseController
    {
        private static readonly User EmailUser = new User("1", "1", default);

        private readonly IEmailFormatter emailFormatter;
        private readonly IEmailTemplateStore emailTemplateStore;

        public object PreviewType { get; private set; }

        public EmailTemplatePreviewController(
            IEmailFormatter emailFormatter,
            IEmailTemplateStore emailTemplateStore)
        {
            this.emailFormatter = emailFormatter;
            this.emailTemplateStore = emailTemplateStore;
        }

        /// <summary>
        /// Get the HTML preview for a channel template.
        /// </summary>
        /// <param name="appId">The id of the app where the templates belong to.</param>
        /// <param name="id">The template ID.</param>
        /// <returns>
        /// 200 => Channel template preview returned.
        /// 404 => Channel template not found.
        /// </returns>
        [HttpGet("api/apps/{appId:notEmpty}/email-templates/{id:notEmpty}/preview")]
        [Produces("text/html")]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<IActionResult> GetPreview(string appId, string id)
        {
            var template = await emailTemplateStore.GetAsync(appId, id, HttpContext.RequestAborted);

            if (template == null || template.Languages.Count == 0)
            {
                return NotFound();
            }

            if (!template.Languages.TryGetValue(App.Language, out var emailTemplate))
            {
                emailTemplate = template.Languages.Values.First();
            }

            var formatted = await FormatAsync(emailTemplate);

            return Content(formatted.Message!.BodyHtml!, "text/html");
        }

        /// <summary>
        /// Render a preview for a email template.
        /// </summary>
        /// <param name="appId">The id of the app where the templates belong to.</param>
        /// <param name="request">The template to render.</param>
        /// <returns>
        /// 200 => Template rendered.
        /// 404 => App not found.
        /// </returns>
        [HttpPost("api/apps/{appId:notEmpty}/email-templates/render")]
        [Produces(typeof(EmailPreviewDto))]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<IActionResult> PostPreview(string appId, [FromBody] EmailPreviewRequestDto request)
        {
            var response = new EmailPreviewDto();

            var formatted = await FormatAsync(request.ToEmailTemplate());

            if (request.Type == EmailPreviewType.Html)
            {
                response.Result = formatted.Message?.BodyHtml;
            }
            else
            {
                response.Result = formatted.Message?.BodyText;
            }

            response.Errors = formatted.Errors?.ToArray();

            return Ok(response);
        }

        private ValueTask<FormattedEmail> FormatAsync(EmailTemplate emailTemplate)
        {
            return emailFormatter.FormatAsync(EmailJob.ForPreview, emailTemplate, App, EmailUser, true, HttpContext.RequestAborted);
        }
    }
}
