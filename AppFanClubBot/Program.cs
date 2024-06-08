namespace AppFanClubBot
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using System.Timers;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using static System.Collections.Specialized.BitVector32;
    using pokenaeBaseClass;

    class Program
    {
        private DiscordSocketClient _client;
        public static CommandService _commands;
        public static IServiceProvider _services;

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();


        char commandstag = '!';
        /// <summary>
        /// リプライによるコマンドを示すタグ
        /// </summary>
        public char CommandsTag
        {
            get { return commandstag; }
            set { commandstag = value; }
        }

        /// <summary>
        /// メインの処理
        /// </summary>
        /// <returns></returns>
        public async Task MainAsync()
        {

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info
                ,
                GatewayIntents = GatewayIntents.All
            });
            _client.Log += Log;
            _commands = new CommandService();
            _services = new ServiceCollection().BuildServiceProvider();

            //スラッシュコマンドを受け取ったときのコマンドを追加する
            _client.SlashCommandExecuted += SlashCommandHandler;

            //jsonファイルから初期設定
            AppFanClubBotSettings settings = JsonExtensions.DeserializeFromFile<AppFanClubBotSettings>(PNBase.ExePath + "settings.json");

            ////テキストを受け取ったときのコマンドを追加する
            //_client.MessageReceived += Commandpokenae;

            //リアクションを受け取ったときのコマンドを追加する
            _client.ReactionAdded += HandleReactionAsync_Add;
            //リアクションが外されたときのコマンドを追加する
            _client.ReactionRemoved += HandleReactionAsync_Remove;

            //ロールが作られたときのイベント
            _client.RoleCreated += ToolCateMakeChannel;

            //次の行に書かれているstring token = "hoge"に先程取得したDiscordTokenを指定する。
            string token = settings.Token;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);

        }

        /// <summary>
        /// ログ
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        /// <summary>
        /// 基本的にはここでコマンドを作る
        /// スラッシュコマンドを受け取ったときのイベント
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            //botのコマンド待機時間を延長
            await command.DeferAsync();

            switch (command.Data.Name)
            {
                case "test"://受信テスト
                    {
                        string Messages = "受信できています．";

                        var myEmb = new EmbedBuilder()
                            .WithTitle("受信テスト") // タイトルを設定
                            .WithDescription(Messages) // 説明を設定
                            .WithColor(Discord.Color.Red) //サイドの色を設定
                            .Build();

                        await command.FollowupAsync(embed: myEmb);

                        break;
                    }
                case "outline"://botの自己紹介・概要
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
                            .WithThumbnailUrl("https://ozaroom.com/pokenaeLogo.png")
                            .Build();

                        await command.FollowupAsync(embed: myEmb);


                        break;
                    }
                case "role"://ロール付与・解除
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
                        var myguild = _client.GetGuild(myguildID);

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

                        break;
                    }
                case "newproject"://新プロジェクト発足時のロール追加とそのお知らせ
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
                            var myguild = _client.GetGuild(myguildID) as IGuild;

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

                                break;
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
                                    .WithAuthor(author,authorIcon) //コマンド実行者の情報を埋め込み
                                    ;

                                var myEmb = myEmbBuild.Build();

                                await command.FollowupAsync(embed: myEmb);
                            }

                        }

                        break;
                    }
                //case "question"://質問の立ち上げ
                //    {
                //        string content = "";
                //        string detail = "";
                //        ulong roleID = 0;
                //        string errormess = "";

                //        foreach (var commandoption in command.Data.Options)
                //        {
                //            if (commandoption.Name == "content")
                //            {
                //                content = commandoption.Value.ToString();

                //            }
                //            else if (commandoption.Name == "detail")
                //            {
                //                detail = commandoption.Value.ToString();

                //            }
                //            else if (commandoption.Name == "projectrole")
                //            {
                //                if (commandoption.Value is Discord.WebSocket.SocketRole)
                //                {
                //                    var myrole = commandoption.Value as Discord.WebSocket.SocketRole;

                //                    if (myrole != null)
                //                    {
                //                        roleID = myrole.Id;

                //                    }

                //                }

                //            }
                //            else if (commandoption.Name == "error")
                //            {
                //                errormess = commandoption.Value.ToString();

                //            }
                //        }

                //        if (projectname != "")
                //        {
                //            ulong myguildID = (ulong)command.GuildId;
                //            var myguild = _client.GetGuild(myguildID) as IGuild;

                //            bool oldroleFlg = false;

                //            foreach (var oldrole in myguild.Roles)
                //            {
                //                if (oldrole.Name == projectname)
                //                {
                //                    oldroleFlg |= true;
                //                    break;
                //                }
                //            }

                //            if (oldroleFlg == true)
                //            {
                //                var myEmbBuild = new EmbedBuilder()
                //                    .WithTitle("エラー") // タイトルを設定
                //                    .WithDescription("既に同名のロールが存在しています．") // 説明を設定
                //                    .WithColor(Discord.Color.Red) //サイドの色を設定
                //                    ;

                //                var myEmb = myEmbBuild.Build();
                //                await command.FollowupAsync(embed: myEmb);

                //                break;
                //            }
                //            else
                //            {
                //                //プロジェクト名と同じカテゴリを生成
                //                await myguild.CreateCategoryAsync(projectname);
                //                //プロジェクト名と同じロールを生成
                //                var newrole = await myguild.CreateRoleAsync(projectname, null, color: Discord.Color.Green, false, null);

                //                //メッセージを送信したユーザー名の取得
                //                var author = command.User.GlobalName;
                //                //メッセージを送信したユーザーのアイコンの取得
                //                var authorIcon = command.User.GetAvatarUrl();

                //                string Messages;
                //                if (multipleFlg == true)
                //                {
                //                    Messages = "@everyone" + Environment.NewLine + "新プロジェクト「" + projectname + "」に参加したい方はロールを取得しましょう！"
                //                     + Environment.NewLine + "ロールを取得するコマンド：" + Environment.NewLine + "/role";
                //                }
                //                else
                //                {
                //                    Messages = "@everyone" + Environment.NewLine + "新プロジェクト「" + projectname + "」に興味のある方はロールを取得しましょう！"
                //                     + Environment.NewLine + "ロールを取得するコマンド：" + Environment.NewLine + "/role";
                //                }

                //                var myEmbBuild = new EmbedBuilder()
                //                    .WithTitle("新規プロジェクトのお知らせ") // タイトルを設定
                //                    .WithDescription(Messages) // 説明を設定
                //                    .WithColor(0x6A5ACD) //サイドの色を設定
                //                    .WithAuthor(author, authorIcon) //コマンド実行者の情報を埋め込み
                //                    ;

                //                var myEmb = myEmbBuild.Build();

                //                await command.FollowupAsync(embed: myEmb);
                //            }

                //        }

                //        break;
                //    }


            }

        }


        //ロール付与のために必要なもの
        /// <summary>
        /// 自由に行き来できるロールの権限の管理
        /// ロールの権限の強さで自由に付与できるか否か決定する
        /// </summary>
        /// <param name="myrole"></param>
        /// <returns></returns>
        public bool RoleNotPermissions(IRole myrole)
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

        
        string[] iconUni = {
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


        /// <summary>
        /// ツール関連のカテゴリが作られたときに自動で基本的なチャンネルを作る
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task ToolCateMakeChannel(SocketRole role)
        {

            if (role.Guild.Id.ToString() == "1247640592129982535" && role.Color == Discord.Color.Green)
            {
                var categoryId = role.Guild.CategoryChannels.First(c => c.Name == role.Name).Id;

                await role.Guild.CreateTextChannelAsync("新機能提案", tcp => tcp.CategoryId = categoryId);
                await role.Guild.CreateTextChannelAsync("質問", tcp => tcp.CategoryId = categoryId);
            }
        }
        /// <summary>
        /// リアクションされたときのイベント
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channel"></param>
        /// <param name="reaction"></param>
        /// <returns></returns>
        private async Task HandleReactionAsync_Add(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var message2 = await message.GetOrDownloadAsync();
            if (message2 != null)
            {
                var context = new CommandContext(_client, message2);

                //IDからリアクションしたユーザーを指定
                ulong id = reaction.UserId;
                var user = await context.Guild.GetUserAsync(id, CacheMode.AllowDownload);

                foreach (Embed embed in message2.Embeds)
                {
                    string embedTitle = embed.Title;
                    string embedDescription = embed.Description;

                    switch (embedTitle)
                    {
                        case "ロール付与・解除":
                            {
                                //ロール付与
                                for (int i = 0; i < embed.Fields.Length; i++)
                                {
                                    if (embed.Fields[i].Name == reaction.Emote.Name)
                                    {
                                        var role = context.Guild.Roles.FirstOrDefault(x => x.Name == embed.Fields[i].Value);
                                        if (role != null)
                                        {
                                            await (user).AddRoleAsync(role);
                                        }
                                    }
                                }

                                break;
                            }
                        default:
                            {
                                
                                break;
                            }

                    }
                }

            }


        }
        /// <summary>
        /// リアクションを外したときのイベント
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channel"></param>
        /// <param name="reaction"></param>
        /// <returns></returns>
        private async Task HandleReactionAsync_Remove(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var message2 = await message.GetOrDownloadAsync();
            if (message2 != null)
            {
                var context = new CommandContext(_client, message2);

                //IDからリアクションしたユーザーを指定
                ulong id = reaction.UserId;
                var user = await context.Guild.GetUserAsync(id, CacheMode.AllowDownload);

                foreach (Embed embed in message2.Embeds)
                {
                    string embedTitle = embed.Title;
                    string embedDescription = embed.Description;


                    switch (embedTitle)
                    {
                        case "ロール付与・解除":
                            {
                                //ロール解除
                                for (int i = 0; i < embed.Fields.Length; i++)
                                {
                                    if (embed.Fields[i].Name == reaction.Emote.Name)
                                    {
                                        var role = context.Guild.Roles.FirstOrDefault(x => x.Name == embed.Fields[i].Value);
                                        if (role != null)
                                        {
                                            await (user).RemoveRoleAsync(role);
                                        }
                                    }
                                }

                                break;
                            }
                        default:
                            {
                                
                                break;
                            }
                    }
                }

            }

        }


    }

}