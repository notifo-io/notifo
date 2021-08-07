// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifo.Domain;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Identity;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Pipeline;
using NSwag.Annotations;
using IEmailTemplateStore = Notifo.Domain.ChannelTemplates.IChannelTemplateStore<Notifo.Domain.Channels.Email.EmailTemplate>;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates
{
    [OpenApiTag("EmailTemplates")]
    public class EmailTemplatePreviewController : BaseController
    {
        private static readonly User EmailUser = new User();
        private static readonly UserNotification[] Notifications =
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
        };

        private readonly IEmailFormatter emailFormatter;
        private readonly IEmailTemplateStore emailTemplateStore;

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
        public async Task<IActionResult> GetPreviewImage(string appId, string id)
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

            var formatted = await emailFormatter.FormatPreviewAsync(Notifications, language, App, EmailUser);

            return Content(formatted.BodyHtml, "text/html");
        }
    }
}
