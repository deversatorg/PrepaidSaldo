using ApplicationAuth.Common.Configs;
using ApplicationAuth.ResourceLibrary;
using ApplicationAuth.Services.Interfaces.Telegram;
using ApplicationAuth.Services.Services.Telegram;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ApplicationAuth.Controllers.API
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("api/bot")]
    public class UpdateController : _BaseApiController
    {
        private readonly ITelegramCoreService _telegramCoreService;
        private readonly IHandleUpdateService _handleUpdate;
        private BotConfiguration _botConfiguration;
        public UpdateController(IStringLocalizer<ErrorsResource> errorsLocalizer , 
                                ITelegramCoreService telegramCoreService,
                                IConfiguration congfiguration,
                                IHandleUpdateService updateService) : base(errorsLocalizer)
        {
            _telegramCoreService = telegramCoreService;
            _botConfiguration = congfiguration.GetSection("BotConfiguration").Get<BotConfiguration>();
            _handleUpdate = updateService;
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody]Update update)
        {
            await _handleUpdate.EchoAsync(update);
            return Ok();
        }
    }
}
