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
    /// 解決策の採用・非採用
    /// </summary>
    public class DiscordReaction_Solution : IDiscordReaction
    {
        public string Command
        {
            get 
            {
                return "Solution";
            }
        }

        public async void ReactionAddEvent(SocketReaction reaction, Embed embed, CommandContext context, IGuildUser user, Cacheable<IUserMessage, ulong> message)
        {
            // コメントがユーザーかBotかの判定
            if (user.IsBot) { return; }

            //解決案を解決策とする
            if (reaction.Emote.Name == "⭕")
            {
                SolutionAPIEdit(1, embed, user, message);
            }
            else if (reaction.Emote.Name == "❌")
            {
                SolutionAPIEdit(2, embed, user, message);
            }
        }

        public async void ReactionRemoveEvent(SocketReaction reaction, Embed embed, CommandContext context, IGuildUser user, Cacheable<IUserMessage, ulong> message)
        {
            // コメントがユーザーかBotかの判定
            if (user.IsBot) { return; }

            //解決策を未確認状態にする
            SolutionAPIEdit(0, embed, user, message);
        }

        /// <summary>
        /// リアクションされた解決案の状態を更新する
        /// </summary>
        /// <param name="state">新しい状態</param>
        /// <param name="embed"></param>
        /// <param name="user"></param>
        /// <param name="message"></param>
        public async void SolutionAPIEdit(int state, Embed embed, IGuildUser user, Cacheable<IUserMessage, ulong> message)
        {
            //メッセージを送信したユーザー名の取得
            var author = user.GlobalName;
            //メッセージを送信したユーザーのアイコンの取得
            var authorIcon = user.GetAvatarUrl();

            var messageRe = await message.GetOrDownloadAsync();
            string messageID = "-";
            string messageID_Q = "-";

            foreach (var myembed in messageRe.Embeds) 
            { 
                foreach(var field in myembed.Fields)
                {
                    if(field.Name == "ID")
                    {
                        messageID = field.Value;
                    }
                    if (field.Name == "QuestionID")
                    {
                        messageID_Q = field.Value;
                    }
                }
            }

            string api_url = "https://script.google.com/macros/s/AKfycbxav3GHPiOiQgDmq3AZ-vF-Fl3pLKRfxdQomMLO0342wtGLgC2XEmkLHsbcwbZkrg8iIQ/exec";
            string url = api_url
                + "?messageid=" + PNBase.Replace_GAS(messageID.ToString())
                + "&state=" + PNBase.Replace_GAS(state.ToString())
                + "&messageid_q=" + PNBase.Replace_GAS(messageID_Q)
                ;

            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            string responseContent = string.Empty;
            using (var responseStream = response.GetResponseStream())
            using (var stRead = new StreamReader(responseStream))
            {
                responseContent = stRead.ReadToEnd();
            }

        }

    }
}
