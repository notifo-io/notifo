// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Events.Dtos;
using Notifo.Domain;
using Notifo.Domain.Events;
using Notifo.Pipeline;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Events
{
    [OpenApiTag("Events")]
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
        /// <returns>
        /// 200 => Events returned.
        /// </returns>
        [HttpGet("api/apps/{appId}/events/")]
        [AppPermission(Roles.Admin)]
        [Produces(typeof(ListResponseDto<EventDto>))]
        public async Task<IActionResult> GetEvents(string appId, [FromQuery] QueryDto q)
        {
            var topics = await eventStore.QueryAsync(appId, q.ToQuery<EventQuery>(), HttpContext.RequestAborted);

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
        /// <returns>
        /// 204 => Events created.
        /// </returns>
        [HttpPost("api/apps/{appId}/events/")]
        [AppPermission(Roles.Admin)]
        public async Task<IActionResult> PostEvents(string appId, [FromBody] PublishManyDto request)
        {
            if (request?.Requests != null)
            {
                foreach (var dto in request.Requests)
                {
                    if (dto != null)
                    {
                        var @event = dto.ToEvent(appId);

                        await eventPublisher.PublishAsync(@event);
                    }
                }
            }

            return NoContent();
        }
    }
}
