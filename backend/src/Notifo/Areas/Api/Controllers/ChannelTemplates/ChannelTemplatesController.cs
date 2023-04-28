// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Identity;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Reflection;
using Notifo.Pipeline;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Notifo.Areas.Api.Controllers.ChannelTemplates;

public abstract class ChannelTemplatesController<T, TDto> : BaseController where T : class, new() where TDto : class, new()
{
    private readonly IChannelTemplateStore<T> channelTemplateStore;

    protected ChannelTemplatesController(IChannelTemplateStore<T> channelTemplateStore)
    {
        this.channelTemplateStore = channelTemplateStore;
    }

    /// <summary>
    /// Get the channel templates.
    /// </summary>
    /// <param name="appId">The id of the app where the templates belong to.</param>
    /// <param name="q">The query object.</param>
    /// <response code="200">Channel templates returned.</response>.
    /// <response code="404">Channel template or app not found.</response>.
    [HttpGet]
    [AppPermission(NotifoRoles.AppAdmin)]
    public async Task<ListResponseDto<ChannelTemplateDto>> GetTemplates(string appId, [FromQuery] QueryDto q)
    {
        var templates = await channelTemplateStore.QueryAsync(appId, q.ToQuery<ChannelTemplateQuery>(true), HttpContext.RequestAborted);

        var response = new ListResponseDto<ChannelTemplateDto>();

        response.Items.AddRange(templates.Select(ChannelTemplateDto.FromDomainObject));
        response.Total = templates.Total;

        return response;
    }

    /// <summary>
    /// Get the channel template by id.
    /// </summary>
    /// <param name="appId">The id of the app where the templates belong to.</param>
    /// <param name="id">The template ID.</param>
    /// <response code="200">Channel templates returned.</response>.
    /// <response code="404">Channel template or app not found.</response>
    [HttpGet("{id:notEmpty}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    public async Task<ChannelTemplateDetailsDto<TDto>> GetTemplate(string appId, string id)
    {
        var template = await channelTemplateStore.GetAsync(appId, id, HttpContext.RequestAborted);

        if (template == null)
        {
            throw new DomainObjectNotFoundException(id);
        }

        return ChannelTemplateDetailsDto<TDto>.FromDomainObject(template, ToDto);
    }

    /// <summary>
    /// Create a channel template.
    /// </summary>
    /// <param name="appId">The id of the app where the templates belong to.</param>
    /// <param name="request">The request object.</param>
    /// <response code="200">Channel template created.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost]
    [AppPermission(NotifoRoles.AppAdmin)]
    public async Task<ChannelTemplateDetailsDto<TDto>> PostTemplate(string appId, [FromBody] CreateChannelTemplateDto request)
    {
        var command = request.ToUpdate<T>(App.Language);

        var template = await Mediator.SendAsync(command, HttpContext.RequestAborted);

        return ChannelTemplateDetailsDto<TDto>.FromDomainObject(template!, ToDto);
    }

    /// <summary>
    /// Create an app template language.
    /// </summary>
    /// <param name="appId">The id of the app where the templates belong to.</param>
    /// <param name="code">The template code.</param>
    /// <param name="request">The request object.</param>
    /// <response code="200">Channel template created.</response>.
    /// <response code="404">Channel template or app not found.</response>.
    [HttpPost("{code:notEmpty}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    public async Task<ChannelTemplateDetailsDto<TDto>> PostTemplateLanguage(string appId, string code, [FromBody] CreateChannelTemplateLanguageDto request)
    {
        var command = request.ToUpdate<T>(code);

        var template = await Mediator.SendAsync(command, HttpContext.RequestAborted);

        return ChannelTemplateDetailsDto<TDto>.FromDomainObject(template!, ToDto);
    }

    /// <summary>
    /// Update an app template.
    /// </summary>
    /// <param name="appId">The id of the app where the templates belong to.</param>
    /// <param name="code">The template code.</param>
    /// <param name="request">The request object.</param>
    /// <response code="204">Channel template updated.</response>.
    /// <response code="404">Channel template or app not found.</response>.
    [HttpPut("{code:notEmpty}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    public async Task<ChannelTemplateDetailsDto<TDto>> PutTemplate(string appId, string code, [FromBody] UpdateChannelTemplateDto<TDto> request)
    {
        var command = request.ToUpdate(code, FromDto);

        var template = await Mediator.SendAsync(command, HttpContext.RequestAborted);

        return ChannelTemplateDetailsDto<TDto>.FromDomainObject(template!, ToDto);
    }

    /// <summary>
    /// Update a channel template language.
    /// </summary>
    /// <param name="appId">The id of the app where the templates belong to.</param>
    /// <param name="code">The template code.</param>
    /// <param name="language">The language.</param>
    /// <param name="request">The request object.</param>
    /// <response code="204">Channel template updated.</response>.
    /// <response code="404">Channel template or app not found.</response>.
    [HttpPut("{code:notEmpty}/{language:notEmpty}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    public async Task<ChannelTemplateDetailsDto<TDto>> PutTemplateLanguage(string appId, string code, string language, [FromBody] TDto request)
    {
        var command = ToUpdate(code, language, request);

        var template = await Mediator.SendAsync(command, HttpContext.RequestAborted);

        return ChannelTemplateDetailsDto<TDto>.FromDomainObject(template!, ToDto);
    }

    /// <summary>
    /// Delete a language channel template.
    /// </summary>
    /// <param name="appId">The id of the app where the templates belong to.</param>
    /// <param name="code">The template ID.</param>
    /// <param name="language">The language.</param>
    /// <response code="204">Channel template updated.</response>.
    /// <response code="404">Channel template or app not found.</response>.
    [HttpDelete("{code:notEmpty}/{language:notEmpty}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    public async Task<ChannelTemplateDetailsDto<TDto>> DeleteTemplateLanguage(string appId, string code, string language)
    {
        var command = ToDelete(code, language);

        var template = await Mediator.SendAsync(command, HttpContext.RequestAborted);

        return ChannelTemplateDetailsDto<TDto>.FromDomainObject(template!, ToDto);
    }

    /// <summary>
    /// Delete a channel template.
    /// </summary>
    /// <param name="appId">The id of the app where the templates belong to.</param>
    /// <param name="code">The template ID.</param>
    /// <response code="204">Channel template deleted.</response>.
    /// <response code="404">Channel template or app not found.</response>.
    [HttpDelete("{code:notEmpty}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteTemplate(string appId, string code)
    {
        var command = new DeleteChannelTemplate<T> { TemplateCode = code };

        await Mediator.SendAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }

    private UpdateChannelTemplateLanguage<T> ToUpdate(string code, string language, TDto request)
    {
        return new UpdateChannelTemplateLanguage<T> { TemplateCode = code, Language = language, Template = FromDto(request) };
    }

    private static DeleteChannelTemplateLanguage<T> ToDelete(string code, string language)
    {
        return new DeleteChannelTemplateLanguage<T> { TemplateCode = code, Language = language };
    }

    protected virtual T FromDto(TDto dto)
    {
        return SimpleMapper.Map(dto, new T());
    }

    protected virtual TDto ToDto(T template)
    {
        return SimpleMapper.Map(template, new TDto());
    }
}
