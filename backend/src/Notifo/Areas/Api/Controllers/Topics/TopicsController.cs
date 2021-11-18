// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Topics.Dtos;
using Notifo.Domain.Identity;
using Notifo.Domain.Topics;
using Notifo.Pipeline;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Topics
{
    [OpenApiTag("Topics")]
    public sealed class TopicsController : BaseController
    {
        private readonly ITopicStore topicStore;

        public TopicsController(ITopicStore topicStore)
        {
            this.topicStore = topicStore;
        }

        /// <summary>
        /// Query topics.
        /// </summary>
        /// <param name="appId">The app where the topics belongs to.</param>
        /// <param name="q">The query object.</param>
        /// <returns>
        /// 200 => Topics returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet("api/apps/{appId}/topics/")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(ListResponseDto<TopicDto>))]
        public async Task<IActionResult> GetTopics(string appId, [FromQuery] QueryDto q)
        {
            var topics = await topicStore.QueryAsync(appId, q.ToQuery<TopicQuery>(true), HttpContext.RequestAborted);

            var response = new ListResponseDto<TopicDto>();

            response.Items.AddRange(topics.Select(TopicDto.FromDomainObject));
            response.Total = topics.Total;

            return Ok(response);
        }
    }
}
