using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFanClubBot
{
    public class AppFanClubBotSettings
    {
        /// <summary>
        /// DiscordBotのトークン
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; }


    }
}
