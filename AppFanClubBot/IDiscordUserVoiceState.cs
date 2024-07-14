using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFanClubBot
{
    public interface IDiscordUserVoiceState
    {
        public void Event(SocketUser user, SocketVoiceState svstate1, SocketVoiceState svstate2)
        {

        }
    }
}
