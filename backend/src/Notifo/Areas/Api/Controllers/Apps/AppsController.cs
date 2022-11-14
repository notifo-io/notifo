// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Apps.Dtos;
using Notifo.Domain.Apps;
using Notifo.Domain.Identity;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure.Security;
using Notifo.Pipeline;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Notifo.Areas.Api.Controllers.Apps;

[ApiExplorerSettings(GroupName = "Apps")]
public sealed class AppsController : BaseController
{
    private readonly IAppStore appStore;
    private readonly IUserResolver userResolver;
    private readonly IIntegrationManager integrationManager;

    public AppsController(IAppStore appStore, IUserResolver userResolver, IIntegrationManager integrationManager)
    {
        this.appStore = appStore;
        this.userResolver = userResolver;
        this.integrationManager = integrationManager;
    }

    /// <summary>
    /// Get the user apps.
    /// </summary>
    /// <response code="200">Apps returned.</response>.
    [HttpGet("api/apps")]
    [AppPermission]
    [Produces(typeof(List<AppDto>))]
    public async Task<IActionResult> GetApps()
    {
        var subject = User.Sub();

        if (string.IsNullOrWhiteSpace(subject))
        {
            return Ok(new List<AppDto>());
        }

        var apps = await appStore.QueryAsync(subject, HttpContext.RequestAborted);

        var response = apps.Select(x => AppDto.FromDomainObject(x, subject)).ToArray();

        return Ok(response);
    }

    /// <summary>
    /// Get app by id.
    /// </summary>
    /// <param name="appId">The id of the app.</param>
    /// <response code="200">Apps returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpGet("api/apps/{appId:notEmpty}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(AppDetailsDto))]
    public async Task<IActionResult> GetApp(string appId)
    {
        var response = await AppDetailsDto.FromDomainObjectAsync(App, UserIdOrSub, userResolver);

        return Ok(response);
    }

    /// <summary>
    /// Create an app.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <response code="200">App created.</response>.
    [HttpPost("api/apps/")]
    [AppPermission]
    [Produces(typeof(AppDto))]
    public async Task<IActionResult> PostApp([FromBody] UpsertAppDto request)
    {
        var command = request.ToUpsert();

        var app = await Mediator.Send(command, HttpContext.RequestAborted);

        var response = AppDto.FromDomainObject(app!, command.PrincipalId);

        return Ok(response);
    }

    /// <summary>
    /// Update an app.
    /// </summary>
    /// <param name="appId">The app id to update.</param>
    /// <param name="request">The request object.</param>
    /// <response code="200">App updated.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost("api/apps/{appId:notEmpty}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(AppDetailsDto))]
    public async Task<IActionResult> PutApp(string appId, [FromBody] UpsertAppDto request)
    {
        var command = request.ToUpsert();

        var app = await Mediator.Send(command, HttpContext.RequestAborted);

        var response = await AppDetailsDto.FromDomainObjectAsync(app!, UserIdOrSub, userResolver);

        return Ok(response);
    }

    /// <summary>
    /// Add an app contributor.
    /// </summary>
    /// <param name="appId">The id of the app.</param>
    /// <param name="request">The request object.</param>
    /// <response code="200">Apps returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost("api/apps/{appId:notEmpty}/contributors")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(AppDetailsDto))]
    public async Task<IActionResult> PostContributor(string appId, [FromBody] AddContributorDto request)
    {
        var command = request.ToUpdate();

        var app = await Mediator.Send(command, HttpContext.RequestAborted);

        var response = await AppDetailsDto.FromDomainObjectAsync(app!, UserIdOrSub, userResolver);

        return Ok(response);
    }

    /// <summary>
    /// Delete an app contributor.
    /// </summary>
    /// <param name="appId">The id of the app.</param>
    /// <param name="contributorId">The contributor to remove.</param>
    /// <response code="200">Apps returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost("api/apps/{appId:notEmpty}/contributors/{contributorId:notEmpty}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(AppDetailsDto))]
    public async Task<IActionResult> DeleteContributor(string appId, string contributorId)
    {
        var command = new RemoveContributor { ContributorId = contributorId };

        var app = await Mediator.Send(command, HttpContext.RequestAborted);

        var response = await AppDetailsDto.FromDomainObjectAsync(app!, UserIdOrSub, userResolver);

        return Ok(response);
    }

    /// <summary>
    /// Get the app integrations.
    /// </summary>
    /// <param name="appId">The id of the app where the integrations belong to.</param>
    /// <response code="200">App email templates returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpGet("api/apps/{appId:notEmpty}/integrations")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(ConfiguredIntegrationsDto))]
    public IActionResult GetIntegrations(string appId)
    {
        var response = ConfiguredIntegrationsDto.FromDomainObject(App, integrationManager);

        return Ok(response);
    }

    /// <summary>
    /// Create an app integrations.
    /// </summary>
    /// <param name="appId">The id of the app where the integration belong to.</param>
    /// <param name="request">The request object.</param>
    /// <response code="200">App integration created.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost("api/apps/{appId:notEmpty}/integration/")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(IntegrationCreatedDto))]
    public async Task<IActionResult> PostIntegration(string appId, [FromBody] CreateIntegrationDto request)
    {
        var command = request.ToUpdate();

        var app = await Mediator.Send(command, HttpContext.RequestAborted);

        var response = IntegrationCreatedDto.FromDomainObject(app!, command.Id);

        return Ok(response);
    }

    /// <summary>
    /// Update an app integration.
    /// </summary>
    /// <param name="appId">The id of the app where the integration belong to.</param>
    /// <param name="id">The id of the integration.</param>
    /// <param name="request">The request object.</param>
    /// <response code="204">App integration updated.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPut("api/apps/{appId:notEmpty}/integrations/{id:notEmpty}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PutIntegration(string appId, string id, [FromBody] UpdateIntegrationDto request)
    {
        var command = request.ToUpdate(id);

        await Mediator.Send(command, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Delete an app integration.
    /// </summary>
    /// <param name="appId">The id of the app where the email templates belong to.</param>
    /// <param name="id">The id of the integration.</param>
    /// <response code="204">App integration deleted.</response>.
    /// <response code="404">App not found.</response>.
    [HttpDelete("api/apps/{appId:notEmpty}/integrations/{id:notEmpty}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteIntegration(string appId, string id)
    {
        var command = new DeleteAppIntegration { IntegrationId = id };

        await Mediator.Send(command, HttpContext.RequestAborted);

        return NoContent();
    }
}
