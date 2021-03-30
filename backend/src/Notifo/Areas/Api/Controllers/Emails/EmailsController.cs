// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Emails.Dto;
using Notifo.Domain;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Emails
{
    [OpenApiIgnore]
    public class EmailsController : BaseController
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

        public EmailsController(IEmailFormatter emailFormatter)
        {
            this.emailFormatter = emailFormatter;
        }

        [HttpPost("/api/email/preview")]
        public async Task<IActionResult> GetEmailPreview([FromBody] RequestPreviewDto request)
        {
            var app = new App
            {
                Name = request.AppName
            };

            try
            {
                if (request.TemplateType == PreviewType.Html)
                {
                    var template = new EmailTemplate
                    {
                        BodyHtml = request.Template
                    };

                    var formatted = await emailFormatter.FormatAsync(Notifications, template, app, EmailUser, true);

                    var response = new PreviewDto
                    {
                        Result = formatted.BodyHtml!
                    };

                    return Ok(response);
                }
                else
                {
                    var template = new EmailTemplate
                    {
                        BodyText = request.Template
                    };

                    var formatted = await emailFormatter.FormatAsync(Notifications, template, app, EmailUser, true);

                    var response = new PreviewDto
                    {
                        Result = formatted.BodyText!
                    };

                    return Ok(response);
                }
            }
            catch (EmailFormattingException ex)
            {
                var response = new PreviewDto
                {
                    Errors = ex.Errors.ToArray()
                };

                return StatusCode(400, response);
            }
        }
    }
}
