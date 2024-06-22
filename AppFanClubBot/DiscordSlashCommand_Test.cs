using Discord;
using Discord.WebSocket;
using pokenaeBaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFanClubBot
{
    /// <summary>
    /// 受信テスト
    /// </summary>
    public class DiscordSlashCommand_Test : IDiscordSlashCommand
    {
        public string Command
        {
            get
            {
                return "test";
            }
        }

        public async void Event(SocketSlashCommand command, DiscordSocketClient client)
        {
            string Messages = "受信できています．";

            var myEmb = new EmbedBuilder()
                .WithTitle("受信テスト") // タイトルを設定
                .WithDescription(Messages) // 説明を設定
                .WithColor(Discord.Color.Red) //サイドの色を設定
                .Build();

            await command.FollowupAsync(embed: myEmb);
        }
    }
}
