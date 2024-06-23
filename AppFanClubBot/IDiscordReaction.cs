using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFanClubBot
{
    /// <summary>
    /// リアクション時のイベント
    /// </summary>
    public interface IDiscordReaction
    {
        /// <summary>
        /// コマンド名
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// リアクションが追加されたときのイベント
        /// </summary>
        public void ReactionAddEvent(SocketReaction reaction, Embed embed, CommandContext context, IGuildUser user, Cacheable<IUserMessage, ulong> message);

        /// <summary>
        /// リアクションが解除されたときのイベント
        /// </summary>
        public void ReactionRemoveEvent(SocketReaction reaction, Embed embed, CommandContext context, IGuildUser user, Cacheable<IUserMessage, ulong> message);
    }
}
