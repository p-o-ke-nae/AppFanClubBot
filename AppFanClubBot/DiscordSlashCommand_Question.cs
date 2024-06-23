using Discord;
using Discord.WebSocket;
using pokenaeBaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using AppFanClubBot;
using System.Data;

namespace AppFanClubBot
{
    /// <summary>
    /// 質問の立ち上げ
    /// </summary>
    public class DiscordSlashCommand_Question : IDiscordSlashCommand
    {
        public string Command
        {
            get
            {
                return "question";
            }
        }

        public async void Event(SocketSlashCommand command, DiscordSocketClient client)
        {
            string content = "-";
            string detail = "-";
            ulong roleID = 0;
            string roleName = "-";
            string errormess = "-";
            string id = "-";

            foreach (var commandoption in command.Data.Options)
            {
                if (commandoption.Name == "content")
                {
                    content = commandoption.Value.ToString();

                }
                else if (commandoption.Name == "detail")
                {
                    detail = commandoption.Value.ToString();

                }
                else if (commandoption.Name == "projectrole")
                {
                    if (commandoption.Value is Discord.WebSocket.SocketRole)
                    {
                        var myrole = commandoption.Value as Discord.WebSocket.SocketRole;

                        if (myrole != null)
                        {
                            roleID = myrole.Id;
                            roleName = myrole.Name;
                        }

                    }

                }
                else if (commandoption.Name == "error")
                {
                    errormess = commandoption.Value.ToString();

                }
            }

            //メッセージを送信したユーザー名の取得
            var author = command.User.GlobalName;
            //メッセージを送信したユーザーのアイコンの取得
            var authorIcon = command.User.GetAvatarUrl();

            //idを生成
            DateTime dateTime = DateTime.Now;
            id = roleID + dateTime.ToString("yyyyMMddhhmmss");


            string api_url = "https://script.google.com/macros/s/AKfycbzaiVXb2GW0oQXYsITxRrxykuEu-SuIfDm_X2M1jK9hnXjf4XDl5FuQT0R7qv9Hc6Jn-Q/exec";
            string url = api_url
                + "?messageid=" + PNBase.Replace_GAS(id)
                + "&content=" + PNBase.Replace_GAS(content)
                + "&detail=" + PNBase.Replace_GAS(detail)
                + "&projectroleid=" + PNBase.Replace_GAS(roleID.ToString())
                + "&error=" + PNBase.Replace_GAS(errormess)
                + "&auther=" + PNBase.Replace_GAS(command.User.GlobalName)
                + "&resolved=" + false
                ;

            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            string responseContent = string.Empty;
            using (var responseStream = response.GetResponseStream())
            using (var stRead = new StreamReader(responseStream))
            {
                responseContent = stRead.ReadToEnd();
            }

            string json = responseContent;
            var newresult = JsonExtensions.DeserializeFromJson<apiResult>(json);

            //Embedの作成
            string Messages = "質問が追加されました．" + Environment.NewLine + "解決策を募集しています．";

            var myEmbBuild = new EmbedBuilder()
                .WithTitle("Question") // タイトルを設定
                ;

            if (newresult != null && newresult.Value == "ok")
            {
                myEmbBuild
                    .AddField("関連プロジェクト・言語", roleName, false)
                    .AddField("ID", id, false)
                    .AddField("内容", content, false)
                    .AddField("詳細", detail, false)
                    .AddField("エラー内容", errormess, false)
                    .WithAuthor(author, authorIcon) //コマンド実行者の情報を埋め込み
                    .WithColor(0x6A5ACD) //サイドの色を設定
                    ;
            }
            else
            {
                Messages = "質問の作成に失敗しました．";

                myEmbBuild.WithColor(Discord.Color.Red); //サイドの色を設定
            }
            myEmbBuild.WithDescription(Messages); // 説明を設定

            var myEmb = myEmbBuild.Build();
            await command.FollowupAsync("<@&" + roleID + ">", embed: myEmb);

        }
    }
}
