using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ApplicationAuth.Common.Attributes;
using ApplicationAuth.Common.Constants;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Models.Enums;
using ApplicationAuth.Models.RequestModels;
using ApplicationAuth.Models.ResponseModels;
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
    [Route("api/v{api-version:apiVersion}/superadmin/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.SuperAdmin)]
    public class AdminsController : _BaseApiController
    {
        private readonly IUserService _userService;

        public AdminsController(IStringLocalizer<ErrorsResource> localizer, 
            IUserService userService)
             : base(localizer)
        {
            _userService = userService;
        }

        // GET api/v1/superadmin/admins
        /// <summary>
        /// Retrieve administrators in pagination
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     
        ///     GET api/v1/superadmin/admins?Search=string&amp;Order.Key=Id&amp;Limit=10&amp;Offset=10
        ///     
        /// </remarks>
        /// <param name="model">Pagination request model</param>
        /// <returns>An administrators list in pagination</returns>
        [HttpGet]
        [SwaggerResponse(200, ResponseMessages.RequestSuccessful, typeof(JsonPaginationResponse<List<UserTableRowResponseModel>>))]
        [SwaggerResponse(400, ResponseMessages.InvalidData, typeof(ErrorResponseModel))]
        [SwaggerResponse(401, ResponseMessages.Unauthorized, typeof(ErrorResponseModel))]
        [SwaggerResponse(403, ResponseMessages.Forbidden, typeof(ErrorResponseModel))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        public IActionResult GetAdministrators([FromQuery]PaginationRequestModel<UserTableColumn> model)
        {
            var data = _userService.GetAll(model, true);

            return Json(new JsonPaginationResponse<List<UserTableRowResponseModel>>(data.Data, model.Offset + model.Limit, data.TotalCount));

        }

    }
}