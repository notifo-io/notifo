// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Identity;
using Notifo.Infrastructure;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates
{
    public abstract class ChannelTemplatesController<T, TDto> : BaseController
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
        /// <returns>
        /// 200 => Channel templates returned.
        /// 404 => Channel template not found.
        /// </returns>
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
        /// <param name="id">The ID of the template.</param>
        /// <returns>
        /// 200 => Channel templates returned.
        /// 404 => Channel template not found.
        /// </returns>
        [HttpGet("{id}")]
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
        /// Create an app template.
        /// </summary>
        /// <param name="appId">The id of the app where the templates belong to.</param>
        /// <param name="request">The request object.</param>
        /// <returns>
        /// 200 => Channel template created.
        /// 404 => Channel template not found.
        /// </returns>
        [HttpPost]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<ChannelTemplateDetailsDto<TDto>> PostTemplate(string appId, [FromBody] CreateChannelTemplateDto request)
        {
            var update = request.ToUpdate<T>();

            var template = await channelTemplateStore.UpsertAsync(appId, null!, update, HttpContext.RequestAborted);

            return ChannelTemplateDetailsDto<TDto>.FromDomainObject(template, ToDto);
        }

        /// <summary>
        /// Create an app template language.
        /// </summary>
        /// <param name="appId">The id of the app where the templates belong to.</param>
        /// <param name="id">The ID of the template.</param>
        /// <param name="request">The request object.</param>
        /// <returns>
        /// 200 => Channel template created.
        /// 404 => Channel template not found.
        /// </returns>
        [HttpPost("{id}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<TDto> PostTemplateLanguage(string appId, string id, [FromBody] CreateChannelTemplateLanguageDto request)
        {
            var update = request.ToUpdate<T>();

            var template = await channelTemplateStore.UpsertAsync(appId, id, update, HttpContext.RequestAborted);

            return ToDto(template.Languages[request.Language]);
        }

        /// <summary>
        /// Update an app template.
        /// </summary>
        /// <param name="appId">The id of the app where the templates belong to.</param>
        /// <param name="id">The ID of the template.</param>
        /// <param name="request">The request object.</param>
        /// <returns>
        /// 204 => Channel template updated.
        /// 404 => Channel template not found.
        /// </returns>
        [HttpPut("{id}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<IActionResult> PutTemplate(string appId, string id, [FromBody] UpdateChannelTemplateDto request)
        {
            var update = request.ToUpdate<T>();

            await channelTemplateStore.UpsertAsync(appId, id, update, HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Update a channel template language.
        /// </summary>
        /// <param name="appId">The id of the app where the templates belong to.</param>
        /// <param name="id">The ID of the template.</param>
        /// <param name="language">The language.</param>
        /// <param name="request">The request object.</param>
        /// <returns>
        /// 204 => Channel template updated.
        /// 404 => Channel template not found.
        /// </returns>
        [HttpPut("{id}/{language}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<IActionResult> PutTemplateLanguage(string appId, string id, string language, [FromBody] TDto request)
        {
            var update = new UpdateChannelTemplate<T>
            {
                Template = FromDto(request)
            };

            update.Language = language;

            await channelTemplateStore.UpsertAsync(appId, id, update, HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Delete a language channel template.
        /// </summary>
        /// <param name="appId">The id of the app where the templates belong to.</param>
        /// <param name="id">The ID of the template.</param>
        /// <param name="language">The language.</param>
        /// <returns>
        /// 204 => Channel template updated.
        /// 404 => Channel template not found.
        /// </returns>
        [HttpDelete("{id}/{language}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<IActionResult> DeleteTemplateLanguage(string appId, string id, string language)
        {
            var update = new DeleteChannelTemplateLanguage<T> { Language = language };

            await channelTemplateStore.UpsertAsync(appId, id, update, HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Delete a channel template.
        /// </summary>
        /// <param name="appId">The id of the app where the templates belong to.</param>
        /// <param name="id">The ID of the template.</param>
        /// <returns>
        /// 204 => Channel template deleted.
        /// 404 => Channel template not found.
        /// </returns>
        [HttpDelete("{id}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<IActionResult> DeleteTemplate(string appId, string id)
        {
            await channelTemplateStore.DeleteAsync(appId, id, HttpContext.RequestAborted);

            return NoContent();
        }

        protected abstract T FromDto(TDto dto);

        protected abstract TDto ToDto(T template);
    }
}
