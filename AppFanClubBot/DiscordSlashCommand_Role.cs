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
    /// ロールの付与・解除
    /// </summary>
    public class DiscordSlashCommand_Role : IDiscordSlashCommand
    {
        public string Command
        {
            get
            {
                return "role";
            }
        }

        public async void Event(SocketSlashCommand command, DiscordSocketClient client)
        {
            IEmote[] emotes = new IEmote[0];
            string description = "";

            var embed = new EmbedBuilder();

            embed.WithTitle("ロール付与・解除");

            embed.WithColor(0x6A5ACD);

            description = "欲しいロールに対応したリアクションを押してください．\n" +
                "ロールを解除したいときはリアクションをもう一度押して外してください．";

            embed.WithDescription(description);

            ulong myguildID = (ulong)command.GuildId;
            var myguild = client.GetGuild(myguildID);

            int roleindex = 0;
            foreach (var myrole in myguild.Roles)
            {
                //チャンネル管理のできないロールのみbotで管理
                if (myrole != null && RoleNotPermissions(myrole) && myrole.Name != "@everyone")
                {
                    embed.AddField(iconUni[roleindex], myrole.Name, false);

                    Array.Resize(ref emotes, roleindex + 1);
                    emotes[roleindex] = new Emoji(iconUni[roleindex]);

                    roleindex++;
                }
                //絵文字の種類数超えたら強制終了
                if (roleindex >= iconUni.Length)
                {
                    break;
                }

            }

            await command.FollowupAsync(embed: embed.Build()).GetAwaiter().GetResult().AddReactionsAsync(emotes);

        }

        //ロール付与のために必要なもの
        /// <summary>
        /// 自由に行き来できるロールの権限の管理
        /// ロールの権限の強さで自由に付与できるか否か決定する
        /// </summary>
        /// <param name="myrole"></param>
        /// <returns></returns>
        private bool RoleNotPermissions(IRole myrole)
        {
            bool result = false;

            //有効な権限の確認
            //https://narikakun.net/technology/discord-developer-permissions/
            result = myrole.Permissions.KickMembers //メンバーをキック
                                                    //|| myrole.Permissions.CreateInstantInvite //招待の作成
                || myrole.Permissions.BanMembers //メンバーをBAN
                || myrole.Permissions.Administrator //管理者(ALL)
                || myrole.Permissions.ManageChannels //チャンネル管理
                || myrole.Permissions.ManageGuild //サーバー管理
                                                  //|| myrole.Permissions.AddReactions //リアクションの追加
                || myrole.Permissions.ViewAuditLog //監視ログの表示
                || myrole.Permissions.PrioritySpeaker //優先スピーカー
                                                      //|| myrole.Permissions.Stream //サーバー内配信
                                                      //|| myrole.Permissions.SendMessages //メッセージ送信
                                                      //|| myrole.Permissions.SendTTSMessages //テキスト読み上げ機能のメッセージ
                || myrole.Permissions.ManageMessages //ピン止めや他メンバーメッセージの削除
                                                     //|| myrole.Permissions.EmbedLinks //埋め込みリンクの送信
                                                     //|| myrole.Permissions.AttachFiles //ファイルの添付
                                                     //|| myrole.Permissions.MentionEveryone //全メンバーへのメンション
                                                     //|| myrole.Permissions.UseExternalEmojis //他サーバの絵文字使用
                                                     //|| myrole.Permissions.ViewGuildInsights //サーバーのインサイトにアクセス
                                                     //|| myrole.Permissions.Connect //音声チャンネルに接続
                                                     //|| myrole.Permissions.Speak //音声チャンネルでの発言
                || myrole.Permissions.MuteMembers //音声チャンネルで他メンバーのミュート
                                                  //|| myrole.Permissions.MoveMembers //チャンネルの移動
                                                  //|| myrole.Permissions.UseVAD //音声検出を使用
                                                  //|| myrole.Permissions.ChangeNickname //自分のニックネームの変更
                || myrole.Permissions.ManageNicknames //他メンバーのニックネーム
                || myrole.Permissions.ManageRoles //ロールの管理
                || myrole.Permissions.ManageWebhooks //ウェブフックの管理
                || myrole.Permissions.ManageEmojisAndStickers //絵文字，スタンプの管理


                ;

            return !(result);
        }

        private string[] iconUni = {
            "\uD83C\uDDE6",
            "\uD83C\uDDE7",
            "\uD83C\uDDE8",
            "\uD83C\uDDE9",
            "\uD83C\uDDEA",
            "\uD83C\uDDEB",
            "\uD83C\uDDEC",
            "\uD83C\uDDED",
            "\uD83C\uDDEE",
            "\uD83C\uDDEF",
            "\uD83C\uDDF0",
            "\uD83C\uDDF1",
            "\uD83C\uDDF2",
            "\uD83C\uDDF3",
            "\uD83C\uDDF4",
            "\uD83C\uDDF5",
            "\uD83C\uDDF6",
            "\uD83C\uDDF7",
            "\uD83C\uDDF8",
            "\uD83C\uDDF9",
            "\uD83C\uDDFA",
            "\uD83C\uDDFB",
            "\uD83C\uDDFC",
            "\uD83C\uDDFD",
            "\uD83C\uDDFE",
            "\uD83C\uDDFF",

        };


    }
}
