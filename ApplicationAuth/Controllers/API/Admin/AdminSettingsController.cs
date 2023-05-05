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
using ApplicationAuth.ResourceLibrary;
using ApplicationAuth.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

namespace ApplicationAuth.Controllers.API.Admin
{
    [Validate]
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("api/v{api-version:apiVersion}/admin-settings")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admins)]
    public class AdminSettingsController : _BaseApiController
    {
        private readonly IAccountService _accountService;

        public AdminSettingsController(IStringLocalizer<ErrorsResource> errorsLocalizer, 
            IAccountService accountService)
            : base(errorsLocalizer)
        {
            _accountService = accountService;
        }

    }
}