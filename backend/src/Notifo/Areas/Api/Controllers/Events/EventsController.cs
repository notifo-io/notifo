// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Events.Dtos;
using Notifo.Domain.Events;
using Notifo.Domain.Identity;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.Events;

[ApiExplorerSettings(GroupName = "Events")]
public sealed class EventsController : BaseController
{
    private readonly IEventStore eventStore;
    private readonly IEventPublisher eventPublisher;

    public EventsController(
        IEventStore eventStore,
        IEventPublisher eventPublisher)
    {
        this.eventStore = eventStore;
        this.eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Query events.
    /// </summary>
    /// <param name="appId">The app where the events belongs to.</param>
    /// <param name="q">The query object.</param>
    /// <response code="200">Events returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpGet("api/apps/{appId:notEmpty}/events/")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(ListResponseDto<EventDto>))]
    public async Task<IActionResult> GetEvents(string appId, [FromQuery] EventQueryDto q)
    {
        var topics = await eventStore.QueryAsync(appId, q.ToQuery(true), HttpContext.RequestAborted);

        var response = new ListResponseDto<EventDto>();

        response.Items.AddRange(topics.Select(x => EventDto.FromDomainObject(x, App)));
        response.Total = topics.Total;

        return Ok(response);
    }

    /// <summary>
    /// Publish events.
    /// </summary>
    /// <param name="appId">The app where the events belongs to.</param>
    /// <param name="request">The publish request.</param>
    /// <response code="204">Events created.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost("api/apps/{appId:notEmpty}/events/")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PostEvents(string appId, [FromBody] PublishManyDto request)
    {
        if (request.Requests?.Length > 100)
        {
            throw new ValidationException($"Only 100 users can be update in one request, found: {request.Requests.Length}");
        }

        foreach (var dto in request.Requests.OrEmpty().NotNull())
        {
            var @event = dto.ToEvent(appId);

            await eventPublisher.PublishAsync(@event, HttpContext.RequestAborted);
        }

        return NoContent();
    }

    /// <summary>
    /// Publish an event for the current user.
    /// </summary>
    /// <param name="request">The publish request.</param>
    /// <response code="204">Event created.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost("api/me/events/")]
    [AppPermission(NotifoRoles.AppUser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PostMyEvents([FromBody] PublishDto request)
    {
        var @event = request.ToEvent(App.Id, $"users/{UserId}");

        await eventPublisher.PublishAsync(@event, HttpContext.RequestAborted);

        return NoContent();
    }
}
