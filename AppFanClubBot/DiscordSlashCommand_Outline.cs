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
    /// botの自己紹介・概要 
    /// </summary>
    public class DiscordSlashCommand_Outline : IDiscordSlashCommand
    {
        public string Command
        {
            get
            {
                return "outline";
            }
        }

        public async void Event(SocketSlashCommand command, DiscordSocketClient client)
        {
            string Messages = "アプリ愛好会補助botです．\n" +
                                "ロール付与などのサポートを主に行っています．\n" +
                                "スラッシュコマンドに対応しているのでそちらよりお申し付けください．\n" +
                                "よろしくお願いします．";

            var myEmb = new EmbedBuilder()
                .WithTitle("自己紹介") // タイトルを設定
                .WithDescription(Messages) // 説明を設定
                .AddField("Manufacture", "ポケなえ", true)
                .AddField("Command", "「/」もしくは「!」(半角)", true)
                .WithColor(0x6A5ACD) //サイドの色を設定
                .Build();

            await command.FollowupAsync(embed: myEmb);
        }
    }
}
