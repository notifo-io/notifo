// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Channels.Email.Formatting;
using Notifo.Domain.Identity;
using Notifo.Pipeline;
using IEmailTemplateStore = Notifo.Domain.ChannelTemplates.IChannelTemplateStore<Notifo.Domain.Channels.Email.EmailTemplate>;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Notifo.Areas.Api.Controllers.ChannelTemplates;

[ApiExplorerSettings(GroupName = "EmailTemplates")]
public class EmailTemplatePreviewController : BaseController
{
    private readonly IEmailFormatter emailFormatter;
    private readonly IEmailTemplateStore emailTemplateStore;
    private readonly MjmlSchema mjmlSchema;

    public object PreviewType { get; private set; }

    public EmailTemplatePreviewController(
        IEmailFormatter emailFormatter,
        IEmailTemplateStore emailTemplateStore,
        MjmlSchema mjmlSchema)
    {
        this.emailFormatter = emailFormatter;
        this.emailTemplateStore = emailTemplateStore;
        this.mjmlSchema = mjmlSchema;
    }

    /// <summary>
    /// Gets the mjml schema.
    /// </summary>
    [HttpGet("api/mjml/schema")]
    [Produces(typeof(MjmlSchema))]
    public IActionResult GetSchema()
    {
        return Ok(mjmlSchema);
    }

    /// <summary>
    /// Get the HTML preview for a channel template.
    /// </summary>
    /// <param name="appId">The id of the app where the templates belong to.</param>
    /// <param name="id">The template ID.</param>
    /// <response code="200">Channel template preview returned.</response>.
    /// <response code="404">Channel template not found.</response>.
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
    /// <response code="200">Template rendered.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost("api/apps/{appId:notEmpty}/email-templates/render")]
    [Produces(typeof(EmailPreviewDto))]
    [AppPermission(NotifoRoles.AppAdmin)]
    public async Task<IActionResult> PostPreview(string appId, [FromBody] EmailPreviewRequestDto request)
    {
        var response = new EmailPreviewDto();

        try
        {
            var formatted = await FormatAsync(request.ToEmailTemplate());

            if (request.Type == EmailPreviewType.Html)
            {
                response.Result = formatted.Message?.BodyHtml;
            }
            else
            {
                response.Result = formatted.Message?.BodyText;
            }

            response.Errors = formatted.Errors?.Select(EmailPreviewErrorDto.FromDomainObject).ToArray();
        }
        catch (EmailFormattingException ex)
        {
            response.Errors = ex.Errors.Select(EmailPreviewErrorDto.FromDomainObject).ToArray();
        }

        return Ok(response);
    }

    private async ValueTask<FormattedEmail> FormatAsync(EmailTemplate emailTemplate)
    {
        await emailFormatter.ParseAsync(emailTemplate, true, HttpContext.RequestAborted);

        return await emailFormatter.FormatAsync(emailTemplate,
            PreviewData.Jobs,
            App,
            PreviewData.User,
            true,
            HttpContext.RequestAborted);
    }
}
