using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace AppFanClubBot
{
    /// <summary>
    /// ロールの付与・解除
    /// </summary>
    public class DiscordReaction_Role : IDiscordReaction
    {
        public string Command
        {
            get
            {
                return "ロール付与・解除";
            }
        }

        public async void ReactionAddEvent(SocketReaction reaction, Embed embed, CommandContext context, IGuildUser user, Cacheable<IUserMessage, ulong> message)
        {
            // コメントがユーザーかBotかの判定
            if (user.IsBot) { return; }

            //ロール付与
            var role = RoleSearch(reaction,embed,context,user);
            if (role != null)
            {
                await (user).AddRoleAsync(role);
            }
        }

        public async void ReactionRemoveEvent(SocketReaction reaction, Embed embed, CommandContext context, IGuildUser user, Cacheable<IUserMessage, ulong> message)
        {
            // コメントがユーザーかBotかの判定
            if (user.IsBot) { return; }

            //ロール解除
            var role = RoleSearch(reaction, embed, context, user);
            if (role != null)
            {
                await (user).RemoveRoleAsync(role);
            }
        }

        /// <summary>
        /// Embedよりロールを検索
        /// </summary>
        public IRole RoleSearch(SocketReaction reaction, Embed embed, CommandContext context, IGuildUser user)
        {
            IRole result = null;

            //対象のロールを検索
            for (int i = 0; i < embed.Fields.Length; i++)
            {
                if (embed.Fields[i].Name == reaction.Emote.Name)
                {
                    var role = context.Guild.Roles.FirstOrDefault(x => x.Name == embed.Fields[i].Value);
                    if (role != null)
                    {
                        result = role;
                    }
                }
            }

            return result;
        }
    }
}
