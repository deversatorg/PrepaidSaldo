using ApplicationAuth.Common.Constants;
using ApplicationAuth.Helpers.Attributes;
using ApplicationAuth.Models.RequestModels;
using ApplicationAuth.Models.ResponseModels;
using ApplicationAuth.Models.ResponseModels.Session;
using ApplicationAuth.ResourceLibrary;
using ApplicationAuth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

namespace ApplicationAuth.Controllers.API.Admin
{
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("api/v{api-version:apiVersion}/admin-verifications")]
    [Validate]
    public class AdminVerificationsController : _BaseApiController
    {
        private readonly IAccountService _accountService;

        public AdminVerificationsController(IStringLocalizer<ErrorsResource> errorsLocalizer, 
            IAccountService accountService)
            : base(errorsLocalizer)
        {
            _accountService = accountService;
        }

    }
}