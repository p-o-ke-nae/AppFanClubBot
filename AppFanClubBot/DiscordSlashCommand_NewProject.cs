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
    /// 新プロジェクト発足時のロール追加とそのお知らせ
    /// </summary>
    public class DiscordSlashCommand_NewProject : IDiscordSlashCommand
    {
        public string Command
        {
            get
            {
                return "newproject";
            }
        }

        public async void Event(SocketSlashCommand command, DiscordSocketClient client)
        {
            string projectname = "";
            bool multipleFlg = false;
            foreach (var commandoption in command.Data.Options)
            {
                if (commandoption.Name == "newprojectname")
                {
                    projectname = commandoption.Value.ToString();

                }
                if (commandoption.Name == "formultiplepeople")
                {
                    multipleFlg = (bool)commandoption.Value;

                }
            }

            if (projectname != "")
            {
                ulong myguildID = (ulong)command.GuildId;
                var myguild = client.GetGuild(myguildID) as IGuild;

                bool oldroleFlg = false;

                foreach (var oldrole in myguild.Roles)
                {
                    if (oldrole.Name == projectname)
                    {
                        oldroleFlg |= true;
                        break;
                    }
                }

                if (oldroleFlg == true)
                {
                    var myEmbBuild = new EmbedBuilder()
                        .WithTitle("エラー") // タイトルを設定
                        .WithDescription("既に同名のロールが存在しています．") // 説明を設定
                        .WithColor(Discord.Color.Red) //サイドの色を設定
                        ;

                    var myEmb = myEmbBuild.Build();
                    await command.FollowupAsync(embed: myEmb);

                }
                else
                {
                    //プロジェクト名と同じカテゴリを生成
                    await myguild.CreateCategoryAsync(projectname);
                    //プロジェクト名と同じロールを生成
                    var newrole = await myguild.CreateRoleAsync(projectname, null, color: Discord.Color.Green, false, null);

                    //メッセージを送信したユーザー名の取得
                    var author = command.User.GlobalName;
                    //メッセージを送信したユーザーのアイコンの取得
                    var authorIcon = command.User.GetAvatarUrl();

                    string Messages;
                    if (multipleFlg == true)
                    {
                        Messages = "@everyone" + Environment.NewLine + "新プロジェクト「" + projectname + "」に参加したい方はロールを取得しましょう！"
                         + Environment.NewLine + "ロールを取得するコマンド：" + Environment.NewLine + "/role";
                    }
                    else
                    {
                        Messages = "@everyone" + Environment.NewLine + "新プロジェクト「" + projectname + "」に興味のある方はロールを取得しましょう！"
                         + Environment.NewLine + "ロールを取得するコマンド：" + Environment.NewLine + "/role";
                    }

                    var myEmbBuild = new EmbedBuilder()
                        .WithTitle("新規プロジェクトのお知らせ") // タイトルを設定
                        .WithDescription(Messages) // 説明を設定
                        .WithColor(0x6A5ACD) //サイドの色を設定
                        .WithAuthor(author, authorIcon) //コマンド実行者の情報を埋め込み
                        ;

                    var myEmb = myEmbBuild.Build();

                    await command.FollowupAsync(embed: myEmb);
                }

            }
        }

    }
}
