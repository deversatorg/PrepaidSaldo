using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ApplicationAuth.Common.Constants;
using ApplicationAuth.Common.Extensions;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Helpers.Attributes;
using ApplicationAuth.Models.RequestModels;
using ApplicationAuth.Models.ResponseModels;
using ApplicationAuth.Models.ResponseModels.Session;
using ApplicationAuth.ResourceLibrary;
using ApplicationAuth.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationAuth.Controllers.API.Admin
{
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("api/v{api-version:apiVersion}/admin-sessions")]
    [Validate]
    public class AdminSessionsController : _BaseApiController
    {
        private readonly IAccountService _accountService;

        public AdminSessionsController(IStringLocalizer<ErrorsResource> errorsLocalizer,
            IAccountService accountService)
            : base(errorsLocalizer)
        {
            _accountService = accountService;
        }

        // POST api/v1/admin-sessions
        /// <summary>
        /// Admin login
        /// </summary>
        /// <remarks>
        /// TEST DATA: 'accessTokenLifetime' - access token lifetime in seconds; ignore it or set value '0' to specify default token lifetime
        /// 
        /// Sample request:
        ///
        ///     POST api/v1/admin-sessions
        ///     {
        ///         "email": "test@example.com",
        ///         "password": "stringG1",
        ///         "accessTokenLifetime": "0"
        ///     }
        /// 
        /// </remarks>
        /// <returns>HTTP 201 with login response or HTTP 400, 500 with error message</returns>
        [SwaggerResponse(201, ResponseMessages.SuccessfulLogin, typeof(JsonResponse<LoginResponseModel>))]
        [SwaggerResponse(400, ResponseMessages.InvalidCredentials, typeof(ErrorResponseModel))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        [SwaggerOperation(Tags = new[] { "Admin Sessions" })]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequestModel model)
        {
            return Created(new JsonResponse<LoginResponseModel>(await _accountService.AdminLogin(model)));
        }

        // DELETE api/v1/admin-sessions
        /// <summary>
        /// Clear admin tokens
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE api/v1/admin-sessions
        ///
        /// </remarks>
        /// <returns>HTTP 200 with success message or HTTP 40X, 500 with error message</returns>
        [SwaggerResponse(200, ResponseMessages.RequestSuccessful, typeof(JsonResponse<MessageResponseModel>))]
        [SwaggerResponse(401, ResponseMessages.Unauthorized, typeof(ErrorResponseModel))]
        [SwaggerResponse(403, ResponseMessages.Forbidden, typeof(ErrorResponseModel))]
        [SwaggerResponse(404, ResponseMessages.NotFound, typeof(ErrorResponseModel))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admins)]
        [SwaggerOperation(Tags = new[] { "Admin Sessions" })]
        [HttpDelete]
        public async Task<IActionResult> Logout()
        {
            await _accountService.Logout(User.GetUserId());

            return Json(new JsonResponse<MessageResponseModel>(new("You have been logged out")));
        }

        // PUT api/v1/admin-sessions
        /// <summary>
        /// Refresh admin's access token
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT api/v1/admin-sessions
        ///     {                
        ///         "refreshToken" : "example-token"
        ///     }
        ///
        /// </remarks>
        /// <returns>HTTP 201 with new access-refresh token pair or HTTP 40X, 500 with error message</returns>
        [SwaggerResponse(201, ResponseMessages.RequestSuccessful, typeof(JsonResponse<TokenResponseModel>))]
        [SwaggerResponse(400, ResponseMessages.InvalidCredentials, typeof(ErrorResponseModel))]
        [SwaggerResponse(403, ResponseMessages.Forbidden, typeof(ErrorResponseModel))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        [SwaggerOperation(Tags = new[] { "Admin Sessions" })]
        [RefreshTokenRoleValidation(new[] { Role.SuperAdmin, Role.Admin })]
        [HttpPut]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestModel model)
        {
            var response = await _accountService.RefreshTokenAsync(model.RefreshToken, new List<string> { Role.SuperAdmin, Role.Admin });

            return Created(new JsonResponse<TokenResponseModel>(response));
        }
    }
}