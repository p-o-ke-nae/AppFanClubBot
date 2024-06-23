using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pokenaeBaseClass;
using System.Net;

namespace AppFanClubBot
{
    /// <summary>
    /// 質問の解決・未解決
    /// </summary>
    public class DiscordReaction_Question : IDiscordReaction
    {
        public string Command
        {
            get 
            {
                return "Question";
            }
        }

        public void ReactionAddEvent(SocketReaction reaction, Embed embed, CommandContext context, IGuildUser user, Cacheable<IUserMessage, ulong> message)
        {
            // コメントがユーザーかBotかの判定
            if (user.IsBot) { return; }

            QuestionAPIEdit(true,message);
        }

        public void ReactionRemoveEvent(SocketReaction reaction, Embed embed, CommandContext context, IGuildUser user, Cacheable<IUserMessage, ulong> message)
        {
            // コメントがユーザーかBotかの判定
            if (user.IsBot) { return; }

            QuestionAPIEdit(false, message);
        }

        /// <summary>
        /// 質問が解決・未解決したことをスプレッドシートに反映する
        /// </summary>
        /// <param name="questionSolve">解決：true,未解決：false</param>
        /// <param name="message"></param>
        public async void QuestionAPIEdit(bool questionSolve, Cacheable<IUserMessage, ulong> message)
        {
            var messageRe = await message.GetOrDownloadAsync();

            //質問を解決済みにする
            string api_url = "https://script.google.com/macros/s/AKfycbzaiVXb2GW0oQXYsITxRrxykuEu-SuIfDm_X2M1jK9hnXjf4XDl5FuQT0R7qv9Hc6Jn-Q/exec";

            //EmbedのFieldより質問IDを取得する
            string messageid = "-";
            if (messageRe != null)
            {
                foreach (var myembed in messageRe.Embeds)
                {
                    foreach (var filed in myembed.Fields)
                    {
                        if (filed.Name == "ID")
                        {
                            messageid = filed.Value.ToString();
                            break;
                        }
                    }
                }
            }


            string url = api_url
                + "?messageid=" + PNBase.Replace_GAS(messageid.ToString())
                + "&resolved=" + questionSolve
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

        }

    }
}
