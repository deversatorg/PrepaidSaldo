using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Common.Configs
{
    public record BotConfiguration
    {
        public string? BotToken { get; init; }

        // Open API is unable to process urls with ":" symbol
        public string? EscapedBotToken => BotToken?.Replace(':', '_');

        public string? HostAddress { get; init; }
    }
}
