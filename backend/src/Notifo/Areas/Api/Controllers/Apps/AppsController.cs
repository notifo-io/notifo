﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Apps.Dtos;
using Notifo.Domain.Apps;
using Notifo.Domain.Identity;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure.Security;
using Notifo.Pipeline;
using NSwag.Annotations;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Notifo.Areas.Api.Controllers.Apps
{
    [OpenApiTag("Apps")]
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
        /// <returns>
        /// 200 => Apps returned.
        /// </returns>
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
        /// <returns>
        /// 200 => Apps returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet("api/apps/{appId}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(AppDetailsDto))]
        public async Task<IActionResult> GetApp(string appId)
        {
            var response = await AppDetailsDto.FromDomainObjectAsync(App, UserId, userResolver);

            return Ok(response);
        }

        /// <summary>
        /// Create an app.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>
        /// 200 => App created.
        /// </returns>
        [HttpPost("api/apps/")]
        [AppPermission]
        [Produces(typeof(AppDto))]
        public async Task<IActionResult> PostApp([FromBody] UpsertAppDto request)
        {
            var subject = UserId;

            if (string.IsNullOrWhiteSpace(subject))
            {
                return Forbid();
            }

            var update = request.ToUpdate(subject);

            var app = await appStore.UpsertAsync(null, update, HttpContext.RequestAborted);

            var response = AppDto.FromDomainObject(app, UserId);

            return Ok(response);
        }

        /// <summary>
        /// Update an app.
        /// </summary>
        /// <param name="appId">The app id to update.</param>
        /// <param name="request">The request object.</param>
        /// <returns>
        /// 200 => App updated.
        /// 404 => App not found.
        /// </returns>
        [HttpPost("api/apps/{appId}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(AppDetailsDto))]
        public async Task<IActionResult> PutApp(string appId, [FromBody] UpsertAppDto request)
        {
            var update = request.ToUpdate(UserId);

            var app = await appStore.UpsertAsync(appId, update, HttpContext.RequestAborted);

            var response = await AppDetailsDto.FromDomainObjectAsync(app, UserId, userResolver);

            return Ok(response);
        }

        /// <summary>
        /// Add an app contributor.
        /// </summary>
        /// <param name="appId">The id of the app.</param>
        /// <param name="request">The request object.</param>
        /// <returns>
        /// 200 => Apps returned.
        /// 404 => App not found.
        /// </returns>
        [HttpPost("api/apps/{appId}/contributors")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(AppDetailsDto))]
        public async Task<IActionResult> PostContributor(string appId, [FromBody] AddContributorDto request)
        {
            var update = request.ToUpdate(UserId);

            var app = await appStore.UpsertAsync(appId, update, HttpContext.RequestAborted);

            var response = await AppDetailsDto.FromDomainObjectAsync(app, UserId, userResolver);

            return Ok(response);
        }

        /// <summary>
        /// Delete an app contributor.
        /// </summary>
        /// <param name="appId">The id of the app.</param>
        /// <param name="contributorId">The contributor to remove.</param>
        /// <returns>
        /// 200 => Apps returned.
        /// 404 => App not found.
        /// </returns>
        [HttpPost("api/apps/{appId}/contributors/{contributorId}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(AppDetailsDto))]
        public async Task<IActionResult> DeleteContributor(string appId, string contributorId)
        {
            var update = new RemoveContributor { ContributorId = contributorId, UserId = UserId };

            var app = await appStore.UpsertAsync(appId, update, HttpContext.RequestAborted);

            var response = await AppDetailsDto.FromDomainObjectAsync(app, UserId, userResolver);

            return Ok(response);
        }

        /// <summary>
        /// Get the app email templates.
        /// </summary>
        /// <param name="appId">The id of the app where the email templates belong to.</param>
        /// <returns>
        /// 200 => App email templates returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet("api/apps/{appId}/email-templates")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(EmailTemplatesDto))]
        public IActionResult GetEmailTemplates(string appId)
        {
            var response = EmailTemplatesDto.FromDomainObject(App);

            return Ok(response);
        }

        /// <summary>
        /// Create an app email template.
        /// </summary>
        /// <param name="appId">The id of the app where the email templates belong to.</param>
        /// <param name="request">The request object.</param>
        /// <returns>
        /// 200 => App email template created.
        /// 404 => App not found.
        /// </returns>
        [HttpPost("api/apps/{appId}/email-templates/")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(EmailTemplateDto))]
        public async Task<IActionResult> PostEmailTemplate(string appId, [FromBody] CreateEmailTemplateDto request)
        {
            var update = request.ToUpdate();

            var app = await appStore.UpsertAsync(appId, update, HttpContext.RequestAborted);

            var response = EmailTemplateDto.FromDomainObject(app.EmailTemplates[request.Language]);

            return Ok(response);
        }

        /// <summary>
        /// Update an app email template.
        /// </summary>
        /// <param name="appId">The id of the app where the email templates belong to.</param>
        /// <param name="language">The language.</param>
        /// <param name="request">The request object.</param>
        /// <returns>
        /// 204 => App email template updated.
        /// 404 => App not found.
        /// </returns>
        [HttpPut("api/apps/{appId}/email-templates/{language}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<IActionResult> PutEmailTemplate(string appId, string language, [FromBody] EmailTemplateDto request)
        {
            var update = request.ToUpdate(language);

            await appStore.UpsertAsync(appId, update, HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Delete an app email template.
        /// </summary>
        /// <param name="appId">The id of the app where the email templates belong to.</param>
        /// <param name="language">The language.</param>
        /// <returns>
        /// 204 => App email template deleted.
        /// 404 => App not found.
        /// </returns>
        [HttpDelete("api/apps/{appId}/email-templates/{language}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<IActionResult> DeleteEmailTemplate(string appId, string language)
        {
            var update = new DeleteAppEmailTemplate { Language = language };

            await appStore.UpsertAsync(appId, update, HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Get the app integrations.
        /// </summary>
        /// <param name="appId">The id of the app where the integrations belong to.</param>
        /// <returns>
        /// 200 => App email templates returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet("api/apps/{appId}/integrations")]
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
        /// <returns>
        /// 200 => App integration created.
        /// 404 => App not found.
        /// </returns>
        [HttpPost("api/apps/{appId}/integration/")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(IntegrationCreatedDto))]
        public async Task<IActionResult> PostIntegration(string appId, [FromBody] CreateIntegrationDto request)
        {
            var update = request.ToUpdate();

            var app = await appStore.UpsertAsync(appId, update, HttpContext.RequestAborted);

            var response = IntegrationCreatedDto.FromDomainObject(app, update.Id);

            return Ok(response);
        }

        /// <summary>
        /// Update an app integration.
        /// </summary>
        /// <param name="appId">The id of the app where the integration belong to.</param>
        /// <param name="id">The id of the integration.</param>
        /// <param name="request">The request object.</param>
        /// <returns>
        /// 204 => App integration updated.
        /// 404 => App not found.
        /// </returns>
        [HttpPut("api/apps/{appId}/integrations/{id}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<IActionResult> PutIntegration(string appId, string id, [FromBody] UpdateIntegrationDto request)
        {
            var update = request.ToUpdate(id);

            await appStore.UpsertAsync(appId, update, HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Delete an app integration.
        /// </summary>
        /// <param name="appId">The id of the app where the email templates belong to.</param>
        /// <param name="id">The id of the integration.</param>
        /// <returns>
        /// 204 => App integration deleted.
        /// 404 => App not found.
        /// </returns>
        [HttpDelete("api/apps/{appId}/integrations/{id}")]
        [AppPermission(NotifoRoles.AppAdmin)]
        public async Task<IActionResult> DeleteIntegration(string appId, string id)
        {
            var update = new DeleteAppIntegration { Id = id };

            await appStore.UpsertAsync(appId, update, HttpContext.RequestAborted);

            return NoContent();
        }
    }
}
