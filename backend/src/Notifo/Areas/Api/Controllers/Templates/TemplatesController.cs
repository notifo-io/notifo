// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Templates.Dtos;
using Notifo.Domain.Identity;
using Notifo.Domain.Templates;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.Templates
{
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
        /// <returns>
        /// 200 => Templates returned.
        /// </returns>
        [HttpGet("api/apps/{appId}/templates/")]
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
        /// <returns>
        /// 200 => Templates upserted.
        /// </returns>
        [HttpPost("api/apps/{appId}/templates/")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(List<TemplateDto>))]
        public async Task<IActionResult> PostTemplates(string appId, [FromBody] UpsertTemplatesDto request)
        {
            var response = new List<TemplateDto>();

            if (request?.Requests != null)
            {
                foreach (var dto in request.Requests)
                {
                    if (dto != null)
                    {
                        var update = dto.ToUpdate();

                        var template = await templateStore.UpsertAsync(appId, dto.Code, update, HttpContext.RequestAborted);

                        response.Add(TemplateDto.FromDomainObject(template));
                    }
                }
            }

            return Ok(response);
        }

        /// <summary>
        /// Delete a template.
        /// </summary>
        /// <param name="appId">The app where the templates belong to.</param>
        /// <param name="code">The template code to delete.</param>
        /// <returns>
        /// 200 => Template deleted.
        /// </returns>
        [HttpDelete("api/apps/{appId}/templates/{code}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(ListResponseDto<TemplateDto>))]
        public async Task<IActionResult> DeleteTemplate(string appId, string code)
        {
            await templateStore.DeleteAsync(appId, code, HttpContext.RequestAborted);

            return NoContent();
        }
    }
}
