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

#pragma warning disable IDE0060 // Remove unused parameter

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
    /// <response code="200">Topics returned.</response>.
    /// <response code="404">App not found.</response>.
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
    /// <response code="200">Named topics upserted.</response>.
    [HttpPost("api/apps/{appId:notEmpty}/topics/")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(List<TopicDto>))]
    public async Task<IActionResult> PostTopics(string appId, [FromBody] UpsertTopicsDto request)
    {
        var response = new List<TopicDto>();

        foreach (var dto in request.Requests.OrEmpty().NotNull())
        {
            var command = dto.ToUpdate();

            var topic = await Mediator.Send(command, HttpContext.RequestAborted);

            response.Add(TopicDto.FromDomainObject(topic!));
        }

        return Ok(response);
    }

    /// <summary>
    /// Delete a topic.
    /// </summary>
    /// <param name="appId">The app where the topics belong to.</param>
    /// <param name="path">The path of the topic to delete.</param>
    /// <response code="204">Topic deleted.</response>.
    [HttpDelete("api/apps/{appId:notEmpty}/topics/{*path}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteTopic(string appId, string path)
    {
        var command = new DeleteTopic { Path = Uri.UnescapeDataString(path) };

        await Mediator.Send(command, HttpContext.RequestAborted);

        return NoContent();
    }
}
