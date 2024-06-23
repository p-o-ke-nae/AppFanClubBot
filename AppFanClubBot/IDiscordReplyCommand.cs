using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace AppFanClubBot
{
    public interface IDiscordReplyCommand
    {
        /// <summary>
        /// コマンド名
        /// </summary>
        public string ReplyCommand { get; }

        /// <summary>
        /// イベントの処理
        /// </summary>
        /// <param name="command"></param>
        public void Event(SocketMessage messageParam, DiscordSocketClient client, SocketUserMessage message, CommandContext context);
    }
}
