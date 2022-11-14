// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Templates.Dtos;
using Notifo.Domain.Identity;
using Notifo.Domain.Templates;
using Notifo.Infrastructure;
using Notifo.Pipeline;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Notifo.Areas.Api.Controllers.Templates;

[ApiExplorerSettings(GroupName = "Templates")]
public sealed class TemplatesController : BaseController
{
    private readonly ITemplateStore templateStore;

    public TemplatesController(ITemplateStore templateStore)
    {
        this.templateStore = templateStore;
    }

    /// <summary>
    /// Query templates.
    /// </summary>
    /// <param name="appId">The app where the templates belongs to.</param>
    /// <param name="q">The query object.</param>
    /// <response code="200">Templates returned.</response>.
    [HttpGet("api/apps/{appId:notEmpty}/templates/")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(ListResponseDto<TemplateDto>))]
    public async Task<IActionResult> GetTemplates(string appId, [FromQuery] QueryDto q)
    {
        var templates = await templateStore.QueryAsync(appId, q.ToQuery<TemplateQuery>(true), HttpContext.RequestAborted);

        var response = new ListResponseDto<TemplateDto>();

        response.Items.AddRange(templates.Select(TemplateDto.FromDomainObject));
        response.Total = templates.Total;

        return Ok(response);
    }

    /// <summary>
    /// Upsert templates.
    /// </summary>
    /// <param name="appId">The app where the templates belong to.</param>
    /// <param name="request">The upsert request.</param>
    /// <response code="200">Templates upserted.</response>.
    [HttpPost("api/apps/{appId:notEmpty}/templates/")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(List<TemplateDto>))]
    public async Task<IActionResult> PostTemplates(string appId, [FromBody] UpsertTemplatesDto request)
    {
        var response = new List<TemplateDto>();

        foreach (var dto in request.Requests.OrEmpty().NotNull())
        {
            var command = dto.ToUpdate(dto.Code);

            var template = await Mediator.Send(command, HttpContext.RequestAborted);

            response.Add(TemplateDto.FromDomainObject(template!));
        }

        return Ok(response);
    }

    /// <summary>
    /// Delete a template.
    /// </summary>
    /// <param name="appId">The app where the templates belong to.</param>
    /// <param name="code">The template code to delete.</param>
    /// <response code="204">Template deleted.</response>.
    [HttpDelete("api/apps/{appId:notEmpty}/templates/{code:notEmpty}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteTemplate(string appId, string code)
    {
        var command = new DeleteTemplate { TemplateCode = code };

        await Mediator.Send(command, HttpContext.RequestAborted);

        return NoContent();
    }
}
