// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;
using Notifo.Domain;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Identity;
using Notifo.Domain.UserNotifications;
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
        private static readonly User EmailUser = new User();
        private static readonly List<EmailJob> EmailJobs = new[]
        {
            new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Subject = "A notification"
                }
            },
            new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Subject = "A notification with body",
                    Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                    ImageLarge = string.Empty,
                    ImageSmall = string.Empty
                }
            },
            new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Subject = "A notification with body and image",
                    Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                    ImageLarge = string.Empty,
                    ImageSmall = "https://via.placeholder.com/150"
                }
            },
            new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Subject = "A notification with body and image and button",
                    Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                    ImageLarge = string.Empty,
                    ImageSmall = "https://via.placeholder.com/150",
                    ConfirmText = "Confirm",
                    ConfirmMode = ConfirmMode.Explicit
                },
                ConfirmUrl = "/url/to/confirm"
            },
            new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Subject = "A notification with body and image and link",
                    Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                    ImageLarge = string.Empty,
                    ImageSmall = "https://via.placeholder.com/150",
                    LinkText = "Follow Link",
                    LinkUrl = "/url/to/link"
                }
            }
        }.Select(x => new EmailJob(x, new NotificationSetting(), "john@internet.com")).ToList();

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
        [HttpGet("api/apps/{appId}/email-templates/{id}/preview")]
        [Produces("text/html")]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<IActionResult> GetPreview(string appId, string id)
        {
            var template = await emailTemplateStore.GetAsync(appId, id, HttpContext.RequestAborted);

            if (template == null || template.Languages.Count == 0)
            {
                return NotFound();
            }

            if (!template.Languages.TryGetValue(App.Language, out var language))
            {
                language = template.Languages.Values.First();
            }

            var formatted = await emailFormatter.FormatPreviewAsync(EmailJobs, language, App, EmailUser, HttpContext.RequestAborted);

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
        [HttpPost("api/apps/{appId}/email-templates/render")]
        [Produces(typeof(EmailPreviewDto))]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<IActionResult> PostPreview(string appId, [FromBody] EmailPreviewRequestDto request)
        {
            var response = new EmailPreviewDto();

            try
            {
                if (request.Type == EmailPreviewType.Html)
                {
                    var template = new EmailTemplate
                    {
                        BodyHtml = request.Template
                    };

                    var formatted = await emailFormatter.FormatPreviewAsync(EmailJobs, template, App, EmailUser, HttpContext.RequestAborted);

                    response.Result = formatted.Message?.BodyHtml;
                    response.Errors = formatted.Errors?.ToArray();

                    return Ok(response);
                }
                else
                {
                    var template = new EmailTemplate
                    {
                        BodyText = request.Template
                    };

                    var formatted = await emailFormatter.FormatPreviewAsync(EmailJobs, template, App, EmailUser, HttpContext.RequestAborted);

                    response.Result = formatted.Message?.BodyHtml;
                    response.Errors = formatted.Errors?.ToArray();
                }
            }
            catch (EmailFormattingException ex)
            {
                response.Errors = ex.Errors?.ToArray();
            }

            return Ok(response);
        }
    }
}
