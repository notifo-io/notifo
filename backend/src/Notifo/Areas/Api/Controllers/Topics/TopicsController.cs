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
using Notifo.Infrastructure;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.Topics;

[ApiExplorerSettings(GroupName = "Topics")]
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
    [HttpGet("api/apps/{appId:notEmpty}/topics/")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(ListResponseDto<TopicDto>))]
    public async Task<IActionResult> GetTopics(string appId, [FromQuery] TopicQueryDto q)
    {
        var topics = await topicStore.QueryAsync(appId, q.ToQuery(true), HttpContext.RequestAborted);

        var response = new ListResponseDto<TopicDto>();

        response.Items.AddRange(topics.Select(TopicDto.FromDomainObject));
        response.Total = topics.Total;

        return Ok(response);
    }

    /// <summary>
    /// Upsert topics.
    /// </summary>
    /// <param name="appId">The app where the topics belong to.</param>
    /// <param name="request">The upsert request.</param>
    /// <returns>
    /// 200 => Named topics upserted.
    /// </returns>
    [HttpPost("api/apps/{appId:notEmpty}/topics/")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(List<TopicDto>))]
    public async Task<IActionResult> PostTopics(string appId, [FromBody] UpsertTopicsDto request)
    {
        var response = new List<TopicDto>();

        foreach (var dto in request.Requests.OrEmpty().NotNull())
        {
            var update = dto.ToUpdate();

            var topic = await topicStore.UpsertAsync(appId, dto.Path, update, HttpContext.RequestAborted);

            response.Add(TopicDto.FromDomainObject(topic));
        }

        return Ok(response);
    }

    /// <summary>
    /// Delete a topic.
    /// </summary>
    /// <param name="appId">The app where the topics belong to.</param>
    /// <param name="id">The ID of the topic to delete.</param>
    /// <returns>
    /// 204 => Topic deleted.
    /// </returns>
    [HttpDelete("api/apps/{appId:notEmpty}/topics/{*id}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(ListResponseDto<TopicDto>))]
    public async Task<IActionResult> DeleteTopic(string appId, string id)
    {
        await topicStore.DeleteAsync(appId, Uri.UnescapeDataString(id), HttpContext.RequestAborted);

        return NoContent();
    }
}
