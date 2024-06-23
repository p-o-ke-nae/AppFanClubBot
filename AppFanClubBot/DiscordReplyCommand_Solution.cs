using Discord;
using Discord.Commands;
using Discord.WebSocket;
using pokenaeBaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AppFanClubBot
{
    public class DiscordReplyCommand_Solution : IDiscordReplyCommand
    {
        public string ReplyCommand
        {
            get
            {
                return "Question";
            }
        }

        public async void Event(SocketMessage messageParam, DiscordSocketClient client, SocketUserMessage message, CommandContext context)
        {
            //EmbedのFieldより質問IDを取得する
            string messageid = "-";

            //コマンドタグを除去したテキストを取得
            string mytext = message.Content.Remove(0, 1);

            foreach (Discord.Embed embed in message.ReferencedMessage.Embeds)
            {
                //タイトルと一致しないなら終了
                if (embed.Title != this.ReplyCommand)
                {
                    return;
                }

                foreach (var filed in embed.Fields)
                {
                    if (filed.Name == "ID")
                    {
                        messageid = filed.Value.ToString();
                        break;
                    }
                }

            }

            //メッセージを送信したユーザー名の取得
            var author = message.Author.GlobalName;
            //メッセージを送信したユーザーのアイコンの取得
            var authorIcon = message.Author.GetAvatarUrl();

            IEmote[] emotes = new IEmote[2];
            emotes[0] = new Emoji(iconUni[0]);
            emotes[1] = new Emoji(iconUni[1]);

            string api_url = "https://script.google.com/macros/s/AKfycbxav3GHPiOiQgDmq3AZ-vF-Fl3pLKRfxdQomMLO0342wtGLgC2XEmkLHsbcwbZkrg8iIQ/exec";
            string url = api_url
                + "?messageid=" + PNBase.Replace_GAS(message.Id.ToString())
                + "&solution=" + PNBase.Replace_GAS(mytext)
                + "&solver=" + PNBase.Replace_GAS(message.Author.GlobalName)
                + "&state=" + PNBase.Replace_GAS(0.ToString())
                + "&messageid_q=" + PNBase.Replace_GAS(messageid)
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
            string Messages = "解決方法が提案されました．";

            var myEmbBuild = new EmbedBuilder()
                .WithTitle("Solution") // タイトルを設定
                ;

            if (newresult != null && newresult.Value == "ok")
            {
                myEmbBuild
                    .AddField("ID", message.Id.ToString(), false)
                    .AddField("QuestionID", messageid, false)
                    .AddField("解決案", mytext, false)
                    .WithAuthor(author, authorIcon) //コマンド実行者の情報を埋め込み
                    .WithColor(0x6A5ACD) //サイドの色を設定
                    ;

                myEmbBuild.WithDescription(Messages); // 説明を設定

                var myEmb = myEmbBuild.Build();
                await context.Channel.SendMessageAsync(embed: myEmb).GetAwaiter().GetResult().AddReactionsAsync(emotes);

            }
            else
            {
                Messages = "解決案の作成に失敗しました．";

                myEmbBuild.WithColor(Discord.Color.Red); //サイドの色を設定

                myEmbBuild.WithDescription(Messages); // 説明を設定

                var myEmb = myEmbBuild.Build();
                await context.Channel.SendMessageAsync(embed: myEmb);

            }

        }

        private string[] iconUni = {
            "⭕",
            "❌",
        };

    }
}
