using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace pokenaeBaseClass
{
    public interface IDiscordSlashCommand
    {
        /// <summary>
        /// コマンド名
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// イベントの処理
        /// </summary>
        /// <param name="command"></param>
        public void Event(SocketSlashCommand command, DiscordSocketClient client);
    }
}
