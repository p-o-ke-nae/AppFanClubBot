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
    using System.Reflection.Metadata;

    class Program
    {
        private DiscordSocketClient _client;
        public static CommandService _commands;
        public static IServiceProvider _services;

        //各コマンドを格納する
        /// <summary>
        /// スラッシュコマンドを格納
        /// </summary>
        private List<IDiscordSlashCommand> SlashCommands = new List<IDiscordSlashCommand>();
        /// <summary>
        /// リアクションによるコマンドを格納
        /// </summary>
        private List<IDiscordReaction> ReactionCommands = new List<IDiscordReaction>();

        private List<IDiscordReplyCommand> ReplyCommands = new List<IDiscordReplyCommand>();

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
            _client.MessageReceived += CommandText;

            //リアクションを受け取ったときのコマンドを追加する
            _client.ReactionAdded += HandleReactionAsync_Add;
            //リアクションが外されたときのコマンドを追加する
            _client.ReactionRemoved += HandleReactionAsync_Remove;

            //ロールが作られたときのイベント
            _client.RoleCreated += ToolCateMakeChannel;

            //コマンドを格納
            //スラッシュコマンド
            SlashCommands.Add(new DiscordSlashCommand_Question());
            SlashCommands.Add(new DiscordSlashCommand_NewProject());
            SlashCommands.Add(new DiscordSlashCommand_Role());
            SlashCommands.Add(new DiscordSlashCommand_Test());
            SlashCommands.Add(new DiscordSlashCommand_Outline());

            //リアクション
            ReactionCommands.Add(new DiscordReaction_Role());
            ReactionCommands.Add(new DiscordReaction_Question());
            ReactionCommands.Add(new DiscordReaction_Solution());

            //リプライコマンド
            ReplyCommands.Add(new DiscordReplyCommand_Solution());

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
        /// スラッシュコマンドを受け取ったときのイベント
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            if (command == null)
            {
                return;
            }
            Console.WriteLine("{0} {1}:{2}\nID:{3}", command.Channel.Name, command.User, command, command.Id);

            //botのコマンド待機時間を延長
            await command.DeferAsync();

            foreach(var discordcommand in SlashCommands)
            {
                if(command.Data.Name == discordcommand.Command)
                {
                    discordcommand.Event(command,_client);

                    break;
                }
            }

        }

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
            if (reaction == null)
            {
                return;
            }
            Console.WriteLine("{0} {1}:{2}\nID:{3}", reaction.Channel.Name, reaction.User, message, message.Id);

            var messageRe = await message.GetOrDownloadAsync();
            if (messageRe != null)
            {
                var context = new CommandContext(_client, messageRe);

                //IDからリアクションしたユーザーを指定
                ulong id = reaction.UserId;
                var user = await context.Guild.GetUserAsync(id, CacheMode.AllowDownload);

                foreach (Embed embed in messageRe.Embeds)
                {
                    string embedTitle = embed.Title;
                    string embedDescription = embed.Description;

                    foreach(var myreaction in ReactionCommands)
                    {
                        if(embedTitle == myreaction.Command)
                        {
                            myreaction.ReactionAddEvent(reaction,embed,context,user,message);
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
            if (reaction == null)
            {
                return;
            }
            Console.WriteLine("{0} {1}:{2}\nID:{3}", reaction.Channel.Name, reaction.User, message, message.Id);

            var messageRe = await message.GetOrDownloadAsync();
            if (messageRe != null)
            {
                var context = new CommandContext(_client, messageRe);

                //IDからリアクションしたユーザーを指定
                ulong id = reaction.UserId;
                var user = await context.Guild.GetUserAsync(id, CacheMode.AllowDownload);

                foreach (Embed embed in messageRe.Embeds)
                {
                    string embedTitle = embed.Title;
                    string embedDescription = embed.Description;

                    foreach (var myreaction in ReactionCommands)
                    {
                        if (embedTitle == myreaction.Command)
                        {
                            myreaction.ReactionRemoveEvent(reaction, embed, context, user, message);
                        }
                    }
                }

            }

        }

        /// <summary>
        /// commandstagによる通常コマンド
        /// </summary>
        /// <param name="messageParam"></param>
        /// <returns></returns>
        private async Task CommandText(SocketMessage messageParam)
        {

            var message = messageParam as SocketUserMessage;
            if (message == null)
            {
                return;
            }
            Console.WriteLine("{0} {1}:{2}\nID:{3}", message.Channel.Name, message.Author.Username, message, message.Id);

            if (message == null) { return; }

            // コメントがユーザーかBotかの判定
            if (message.Author.IsBot) { return; }


            int argPos = 0;

            // コマンドかどうか判定（今回は、「!」で判定）
            if (!(message.HasCharPrefix(CommandsTag, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) { return; }

            var context = new CommandContext(_client, message);

            // 実行
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            //実行できなかった場合
            if (!result.IsSuccess)
            {
                //EmbedのFieldより質問IDを取得する
                string messageid = "-";

                //コマンドタグを除去したテキストを取得
                string mytext = message.Content.Remove(0, 1);

                if (message.ReferencedMessage != null)
                {
                    //リプライコマンドを実行する対象のEmbedを取得する
                    if (message.ReferencedMessage.Embeds.Count == 1)
                    {
                        foreach (Discord.Embed embed in message.ReferencedMessage.Embeds)
                        {
                            //リプライコマンドなのでリプライ対象のコマンドを読み取る
                            foreach (var myreplycommand in ReplyCommands)
                            {
                                if (embed.Title == myreplycommand.ReplyCommand)
                                {
                                    myreplycommand.Event(messageParam, _client, message, context);
                                }
                            }
                        }
                    }
                }

            }
        }




        public class AFCQA_QA
        {
            /// <summary>
            /// 質問メッセージのID
            /// </summary>
            [JsonProperty("messageid")]
            public string MessageID { get; set; }

            /// <summary>
            /// 質問内容
            /// </summary>
            [JsonProperty("content")]
            public string Content { get; set; }

            /// <summary>
            /// 質問詳細
            /// </summary>
            [JsonProperty("detail")]
            public string Detail { get; set; }

            /// <summary>
            /// プロジェクトロールのID
            /// </summary>
            [JsonProperty("projectroleid")]
            public ulong ProjectRoleID { get; set; }

            /// <summary>
            /// エラー内容
            /// </summary>
            [JsonProperty("error")]
            public string Error { get; set; }

            /// <summary>
            /// 質問者
            /// </summary>
            [JsonProperty("auther")]
            public string Auther { get; set; }

            /// <summary>
            /// 解決したか否か
            /// </summary>
            [JsonProperty("resolved")]
            public bool Resolved { get; set; }

            /// <summary>
            /// 解決策リスト
            /// </summary>
            [JsonProperty("answers")]
            public List<AFCQA_Solution> Solutions { get; set; }

        }

        public class AFCQA_Solution
        {
            /// <summary>
            /// 解決案メッセージのID
            /// </summary>
            [JsonProperty("messageid")]
            public string MessageID { get; set; }

            /// <summary>
            /// 解決案
            /// </summary>
            [JsonProperty("solution")]
            public string Solution { get; set; }

            /// <summary>
            /// 解決者
            /// </summary>
            [JsonProperty("solver")]
            public string Solver { get; set; }

            /// <summary>
            /// 解決案の状態
            /// 0:保留，1:解決した，2:解決しなかった
            /// </summary>
            [JsonProperty("state")]
            public string State { get; set; }
        }

        

    }

    public class apiResult
    {
        /// <summary>
        /// GASの結果
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; } = "";
    }


}