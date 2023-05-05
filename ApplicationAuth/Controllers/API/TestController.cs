using ApplicationAuth.Common.Attributes;
using ApplicationAuth.Common.Constants;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Domain.Entities.Telegram;
using ApplicationAuth.Helpers.Attributes;
using ApplicationAuth.Models.RequestModels.Test;
using ApplicationAuth.Models.ResponseModels;
using ApplicationAuth.Models.ResponseModels.Saldo;
using ApplicationAuth.Models.ResponseModels.Session;
using ApplicationAuth.ResourceLibrary;
using ApplicationAuth.Services.Interfaces;
using ApplicationAuth.Services.Interfaces.Telegram;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ApplicationAuth.Controllers.API
{
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    [Validate]
    public class TestController : _BaseApiController
    {
        private readonly ILogger<TestController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJWTService _jwtService;
        private readonly IUserService _userService;
        private readonly ITelegramCoreService _telegramCoreService;
        private readonly ISaldoService _saldoService;

        public TestController(IStringLocalizer<ErrorsResource> localizer,
            ILogger<TestController> logger,
            IUnitOfWork unitOfWork,
            IJWTService jwtService,
            IUserService userService,
            IServiceProvider serviceProvider,
            ITelegramCoreService telegramCoreService,
            ISaldoService saldoService
            )
            : base(localizer)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _userService = userService;
            _telegramCoreService = telegramCoreService;
            _saldoService = saldoService;
        }

        /// <summary>
        /// For Swagger UI
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //[ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("authorize")]
        public async Task<IActionResult> AuthorizeWithoutCredentials([FromBody] ShortAuthorizationRequestModel model)
        {
            IQueryable<ApplicationUser> users = null;

            if (model.Id.HasValue)
                users = _unitOfWork.Repository<ApplicationUser>().Get(x => x.Id == model.Id);
            else if (!string.IsNullOrEmpty(model.UserName))
                users = _unitOfWork.Repository<ApplicationUser>().Get(x => x.UserName == model.UserName);

            var user = await users.Include(x => x.Profile).FirstOrDefaultAsync();

            if (user == null)
            {
                Errors.AddError("", "User is not found");
                return Errors.Error(HttpStatusCode.NotFound);
            }

            var result = await _jwtService.BuildLoginResponse(user);

            return Json(new JsonResponse<LoginResponseModel>(result));
        }

        // GET api/v1/test/saldo
        /// <summary>
        /// Get balance 
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE api/v1/test/DeleteAccount?userid=1
        ///
        /// </remarks>
        /// <returns>HTTP 200 with success message or HTTP 40X, 500 with message error</returns>
        [HttpGet("saldo")]
        [SwaggerResponse(200, ResponseMessages.RequestSuccessful, typeof(JsonResponse<MessageResponseModel>))]
        [SwaggerResponse(400, ResponseMessages.InvalidData, typeof(ErrorResponseModel))]
        [SwaggerResponse(401, ResponseMessages.Unauthorized, typeof(ErrorResponseModel))]
        [SwaggerResponse(403, ResponseMessages.Forbidden, typeof(ErrorResponseModel))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        public async Task<IActionResult> DeleteAccount([FromQuery]string telegramId)
        {
            var response = await _saldoService.Get(telegramId);
            return Json(new JsonResponse<SaldoResponseModel>(response));
        }

        // DELETE api/v1/test/DeleteAccount
        /// <summary>
        /// Hard delete user from db
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE api/v1/test/DeleteAccount?userid=1
        ///
        /// </remarks>
        /// <returns>HTTP 200 with success message or HTTP 40X, 500 with message error</returns>
        [HttpDelete("DeleteAccount")]
        [SwaggerResponse(200, ResponseMessages.RequestSuccessful, typeof(JsonResponse<MessageResponseModel>))]
        [SwaggerResponse(400, ResponseMessages.InvalidData, typeof(ErrorResponseModel))]
        [SwaggerResponse(401, ResponseMessages.Unauthorized, typeof(ErrorResponseModel))]
        [SwaggerResponse(403, ResponseMessages.Forbidden, typeof(ErrorResponseModel))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        public async Task<IActionResult> DeleteAccount([FromQuery][ValidateId] int userId)
        {
            await _userService.HardDeleteUser(userId);
            return Json(new JsonResponse<MessageResponseModel>(new("User has been deleted")));
        }

        // DELETE api/v1/test/stickerRate
        /// <summary>
        /// Hard delete telegramStickers from db
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE api/v1/test/stickerRate
        ///
        /// </remarks>
        /// <returns>HTTP 200 with success message or HTTP 40X, 500 with message error</returns>
        [HttpDelete("stickerRate")]
        [SwaggerResponse(200, ResponseMessages.RequestSuccessful, typeof(JsonResponse<MessageResponseModel>))]
        [SwaggerResponse(400, ResponseMessages.InvalidData, typeof(ErrorResponseModel))]
        [SwaggerResponse(500, ResponseMessages.InternalServerError, typeof(ErrorResponseModel))]
        public async Task<IActionResult> DeleteTelegramStickerRate()
        {
            var ids = _unitOfWork.Repository<TelegramSticker>().GetAll().Select(x => x.Id).ToList();

            foreach (var id in ids)
            {
                 _unitOfWork.Repository<TelegramSticker>().DeleteById(id);
            }
            _unitOfWork.SaveChanges();

            return Json(new JsonResponse<MessageResponseModel>(new("Stickers was deleted from db")));
        }

    }
}