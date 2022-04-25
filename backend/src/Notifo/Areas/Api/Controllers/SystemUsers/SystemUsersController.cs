﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.SystemUsers.Dtos;
using Notifo.Domain.Identity;
using Notifo.Identity;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Security;
using Notifo.Pipeline;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.SystemUsers
{
    [OpenApiTags("SystemUsers")]
    public sealed class SystemUsersController : BaseController
    {
        private readonly IUserService userService;

        public SystemUsersController(IUserService userService)
        {
            this.userService = userService;
        }

        /// <summary>
        /// Query users.
        /// </summary>
        /// <param name="q">The query object.</param>
        /// <returns>
        /// 200 => Users returned.
        /// </returns>
        [HttpGet("api/system-users/")]
        [ProducesResponseType(typeof(ListResponseDto<SystemUserDto>), StatusCodes.Status200OK)]
        [AppPermission(NotifoRoles.HostAdmin)]
        public async Task<IActionResult> GetUsers([FromQuery] QueryDto q)
        {
            var users = await userService.QueryAsync(q.Query, q.Take, q.Skip, HttpContext.RequestAborted);

            var response = new ListResponseDto<SystemUserDto>();

            response.Items.AddRange(users.Select(x => SystemUserDto.FromDomainObject(x, !IsUser(x.Id))));
            response.Total = users.Total;

            return Ok(response);
        }

        /// <summary>
        /// Get a user.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>
        /// 200 => User returned.
        /// 404 => User not found.
        /// </returns>
        [HttpGet("api/system-users/{id:notEmpty}/")]
        [ProducesResponseType(typeof(SystemUserDto), 201)]
        [AppPermission(NotifoRoles.HostAdmin)]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await userService.FindByIdAsync(id, HttpContext.RequestAborted);

            if (user == null)
            {
                return NotFound();
            }

            var response = SystemUserDto.FromDomainObject(user, !IsUser(user.Id));

            return Ok(response);
        }

        /// <summary>
        /// Create a user.
        /// </summary>
        /// <param name="request">The create request.</param>
        /// <returns>
        /// 201 => User created.
        /// </returns>
        [HttpPost("api/system-users/")]
        [ProducesResponseType(typeof(SystemUserDto), 201)]
        [AppPermission(NotifoRoles.HostAdmin)]
        public async Task<IActionResult> PostUser([FromBody] CreateSystemUserDto request)
        {
            var user = await userService.CreateAsync(request.Email, request.ToValues(), ct: HttpContext.RequestAborted);

            var response = SystemUserDto.FromDomainObject(user, !IsUser(user.Id));

            return Ok(response);
        }

        /// <summary>
        /// Update the user.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <param name="request">The update request.</param>
        /// <returns>
        /// 200 => User updated.
        /// 403 => User cannot be updated.
        /// </returns>
        [HttpPut("api/system-users/{id:notEmpty}/")]
        [ProducesResponseType(typeof(SystemUserDto), 200)]
        [AppPermission(NotifoRoles.HostAdmin)]
        public async Task<IActionResult> PutUser(string id, [FromBody] UpdateSystemUserDto request)
        {
            if (IsUser(id))
            {
                throw new DomainForbiddenException("You cannot update yourself.");
            }

            var user = await userService.UpdateAsync(id, request.ToValues(), ct: HttpContext.RequestAborted);

            var response = SystemUserDto.FromDomainObject(user, !IsUser(user.Id));

            return Ok(response);
        }

        /// <summary>
        /// Lock the user.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>
        /// 200 => User locked.
        /// 403 => User cannot be locked.
        /// </returns>
        [HttpPut("api/system-users/{id:notEmpty}/lock/")]
        [ProducesResponseType(typeof(SystemUserDto), 200)]
        [AppPermission(NotifoRoles.HostAdmin)]
        public async Task<IActionResult> LockUser(string id)
        {
            if (IsUser(id))
            {
                throw new DomainForbiddenException("You cannot lock yourself.");
            }

            var user = await userService.LockAsync(id, HttpContext.RequestAborted);

            var response = SystemUserDto.FromDomainObject(user, !IsUser(user.Id));

            return Ok(response);
        }

        /// <summary>
        /// Unlock the user.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>
        /// 200 => User unlocked.
        /// 403 => User cannot be unlocked.
        /// </returns>
        [HttpPut("api/system-users/{id:notEmpty}/unlock/")]
        [ProducesResponseType(typeof(SystemUserDto), 200)]
        [AppPermission(NotifoRoles.HostAdmin)]
        public async Task<IActionResult> UnlockUser(string id)
        {
            if (IsUser(id))
            {
                throw new DomainForbiddenException("You cannot unlock yourself.");
            }

            var user = await userService.UnlockAsync(id, HttpContext.RequestAborted);

            var response = SystemUserDto.FromDomainObject(user, !IsUser(user.Id));

            return Ok(response);
        }

        /// <summary>
        /// Delete the user.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>
        /// 204 => User deleted.
        /// 403 => User cannot be deleted.
        /// </returns>
        [HttpDelete("api/system-users/{id:notEmpty}/")]
        [AppPermission(NotifoRoles.HostAdmin)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (IsUser(id))
            {
                throw new DomainForbiddenException("You cannot update yourself.");
            }

            await userService.DeleteAsync(id, HttpContext.RequestAborted);

            return NoContent();
        }

        private bool IsUser(string userId)
        {
            var subject = User.Sub();

            return string.Equals(subject, userId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
