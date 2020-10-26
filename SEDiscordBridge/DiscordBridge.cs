using DSharpPlus;
using DSharpPlus.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Torch.API.Managers;
using Torch.API.Session;
using Torch.Commands;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace SEDiscordBridge
{
    public partial class DiscordBridge
    {
        private static SEDiscordBridgePlugin Plugin;
        private DiscordGame game = new DiscordGame();
        private string lastMessage = "";
        private ulong botId = 0;
        private int retry = 0;

        public bool Ready { get; set; } = false;
        public static DiscordClient Discord { get; set; }

        public static int Cooldown;
        public static decimal Increment;
        public static decimal Factor;
        public static decimal CooldownNeutral;
        public static int FirstWarning;
        public static decimal MinIncrement;
        public static decimal Locked;

        private Thread _deathLogThread;
        private readonly Queue<DeathMessage> _deathMessagesStack = new Queue<DeathMessage>();

        public DiscordBridge(SEDiscordBridgePlugin plugin)
        {
            Plugin = plugin;

            _deathLogThread = new Thread(DeathLogThread);


            Cooldown = plugin.Config.SimCooldown;
            Increment = (plugin.Config.StatusInterval / 1000);
            Factor = plugin.Config.SimCooldown / Increment;
            Increment = plugin.Config.SimCooldown / Increment;
            MinIncrement = 60 / (plugin.Config.StatusInterval / 1000);
            Locked = 0;
            Task.Run(() => RegisterDiscord());
        }

        private async void RunGameTask(Action obj)
        {
            if (Plugin.Torch.CurrentSession != null)
            {
                await Plugin.Torch.InvokeAsync(obj);
            }
            else
            {
                await Task.Run(obj);
            }
        }
        public void StopDiscord()
        {
            DisconnectDiscord();
        }

        private void DisconnectDiscord()
        {
            Ready = false;
            Discord?.DisconnectAsync();
        }

        private Task RegisterDiscord()
        {
            try
            {
                // Windows Vista - 8.1
                if (Environment.OSVersion.Platform.Equals(PlatformID.Win32NT) && Environment.OSVersion.Version.Major == 6)
                {
                    Discord = new DiscordClient(new DiscordConfiguration
                    {
                        Token = Plugin.Config.BotToken,
                        TokenType = TokenType.Bot,
                    });
                }
                else
                {
                    Discord = new DiscordClient(new DiscordConfiguration
                    {
                        Token = Plugin.Config.BotToken,
                        TokenType = TokenType.Bot
                    });
                }
            }
            catch (Exception) { }

            Discord.ConnectAsync();

            Discord.MessageCreated += Discord_MessageCreated;

            Discord.Ready += async e =>
            {
                Ready = true;
                _deathLogThread.Start();
                await Task.CompletedTask;
            };
            return Task.CompletedTask;
        }

        public void SendStatus(string status, UserStatus userStatus)
        {
            if (Ready && status?.Length > 0)
            {
                game.Name = status;
                Task.Run(() => Discord.UpdateStatusAsync(game, userStatus));
            }
        }

        public void SendSimMessage(string msg)
        {
            try
            {
                if (Ready && Plugin.Config.SimChannel.Length > 0)
                {
                    DiscordChannel chann = Discord.GetChannelAsync(ulong.Parse(Plugin.Config.SimChannel)).Result;
                    //mention
                    //msg = MentionNameToID(msg, chann);
                    msg = Plugin.Config.SimMessage.Replace("{ts}", TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToString());
                    botId = Discord.SendMessageAsync(chann, msg.Replace("/n", "\n")).Result.Author.Id;
                }
            }
            catch (Exception e)
            {
                SEDiscordBridgePlugin.Log.Error(e);
            }
        }
        public void RunSendChatTask(string user, string msg)
        {
            Task.Run(() => SendChatMessage(user, msg));
        }

        public void RunSendFacTask(string user, string msg, string facName)
        {
            Task.Run(() => SendFacChatMessage(user, msg, facName));
        }

        public async Task SendChatMessage(string user, string msg)
        {
            if (lastMessage.Equals(user + msg)) return;

            if (Ready && Plugin.Config.ChatChannelId.Length > 0)
            {
                DiscordChannel chann = Discord.GetChannelAsync(ulong.Parse(Plugin.Config.ChatChannelId)).Result;
                //mention
                msg = MentionNameToID(msg, chann);

                if (user != null)
                {
                    msg = Plugin.Config.Format.Replace("{msg}", msg).Replace("{p}", user).Replace("{ts}", TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToString());
                }
                try
                {
                    await Discord.SendMessageAsync(chann, msg.Replace("/n", "\n"));
                }
                catch (DSharpPlus.Exceptions.RateLimitException)
                {
                    if (retry <= 5)
                    {
                        retry++;
                        await SendChatMessage(user, msg);
                        retry = 0;
                    }
                    else
                    {
                        SEDiscordBridgePlugin.Log.Fatal($"Aborting send chat message (Too many attempts)");
                        SEDiscordBridgePlugin.Log.Warn($"Message: {msg}");
                    }
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    SEDiscordBridgePlugin.Log.Fatal($"Unable to send message");
                    SEDiscordBridgePlugin.Log.Warn($"Message: {msg}");
                }
            }
        }

        public void CreateDeathMessage(MyEntity attacker, DamageType damageType, MyEntity target, EntityType targetType, MyPlayer attackerOwner, MyPlayer targetOwner)
        {
            if (!Plugin.Config.DeathsEnabled) return;
            _deathMessagesStack.Enqueue(new DeathMessage()
            {
                Attacker = attacker,
                DamageType = damageType,
                Target = target,
                TargetType = targetType,
                AttackerOwner = attackerOwner,
                TargetOwner = targetOwner
            });
        }

        private async Task SendDeathMessageInternal(string message, ulong id)
        {
            var channel = await Discord.GetChannelAsync(id);
            await Discord.SendMessageAsync(channel, message);
        }

        public void DeathLogThread()
        {
            try {
                while (true) {
                    if (_deathMessagesStack.Count > 0) {
                        var messages = new Dictionary<EntityType, StringBuilder>
                        {
                        { EntityType.Character, new StringBuilder() },
                        { EntityType.Grid, new StringBuilder() }
                    };
                        foreach (var group in _deathMessagesStack.GroupBy(b => b.Target)) {
                            var topMostAttaker = group.GroupBy(a => a.Attacker).OrderByDescending(b => b.Key).FirstOrDefault();
                            var tagetType = group.GroupBy(b => b.TargetType).OrderByDescending(b => b.Key).FirstOrDefault();
                            var topTargetOwner = group.GroupBy(a => a.TargetOwner).OrderByDescending(b => b.Key).FirstOrDefault();
                            var topAttackOwner = group.GroupBy(a => a.AttackerOwner).OrderByDescending(b => b.Key).FirstOrDefault();
                            var damageType = group.GroupBy(a => a.DamageType).OrderByDescending(b => b.Key).FirstOrDefault();

                            messages[tagetType.Key].AppendLine(DamageTexts.Formate(new DeathMessage() {
                                Target = group.Key,
                                TargetOwner = topTargetOwner.Key,
                                TargetType = tagetType.Key,
                                Attacker = topMostAttaker.Key,
                                AttackerOwner = topAttackOwner.Key,
                                DamageType = damageType.Key,
                            }, group.Count()));
                        }
                        _deathMessagesStack.Clear();

                        Plugin.Config.DeathRoutes.ToList()
                            .Where(b => messages[b.EntityType].Length > 10)
                            .Select(b => SendDeathMessageInternal(messages[b.EntityType].ToString(), b.ChannelId))
                            .ForEach(b => Task.Run(() => b));
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(15));
                }
            }
            catch (Exception e) {
                SEDiscordBridgePlugin.Log.Error(e.ToString());
            }
        }

        public async Task SendFacChatMessage(string user, string msg, string facName)
        {
            try
            {
                var channelIds = Plugin.Config.FactionChannels.Where(c => c.Faction.Equals(facName));
                if (Ready && channelIds.Count() > 0)
                {
                    foreach (var chId in channelIds)
                    {
                        DiscordChannel chann = await Discord.GetChannelAsync(chId.Channel);
                        //mention
                        msg = MentionNameToID(msg, chann);

                        if (user != null)
                        {
                            msg = Plugin.Config.FacFormat.Replace("{msg}", msg).Replace("{p}", user).Replace("{ts}", TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToString());
                        }
                        var res = await Discord.SendMessageAsync(chann, msg.Replace("/n", "\n"));
                        botId = res.Author.Id;
                    }
                }
            }
            catch (Exception e)
            {
                SEDiscordBridgePlugin.Log.Error($"SendFacChatMessage: {e.Message}");
            }
        }

        public async void SendStatusMessage(string user, string msg, Torch.API.IPlayer obj = null)
        {
            if (Ready && Plugin.Config.StatusChannelId.Length > 0)
            {
                try
                {
                    DiscordChannel chann = await Discord.GetChannelAsync(ulong.Parse(Plugin.Config.StatusChannelId));

                    if (user != null)
                    {
                        if (user.StartsWith("ID:"))
                            return;

                        if (obj != null && Plugin.Config.DisplaySteamId)
                        {
                            user = $"{user} ({obj.SteamId})";
                        }

                        msg = msg.Replace("{p}", user).Replace("{ts}", TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToString());
                    }
                    botId = (await Discord.SendMessageAsync(chann, msg.Replace("/n", "\n"))).Author.Id;
                }
                catch (Exception e)
                {
                    SEDiscordBridgePlugin.Log.Error($"SendStatusMessage: {e.Message}");
                }
            }
        }

        private async Task Discord_MessageCreated(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            bool cmdConditionMatch = false;
            dynamic cmdPrefixes = Plugin.Config.CommandPrefix;
            string matchedPrefix = "";
            cmdPrefixes = cmdPrefixes.Split();

            if (!e.Author.IsBot || (!botId.Equals(e.Author.Id) && Plugin.Config.BotToGame))
            {
                string comChannelId = Plugin.Config.CommandChannelId;
                if (!string.IsNullOrEmpty(comChannelId))
                {

                    foreach (string prefix in cmdPrefixes)
                    {
                        if (Plugin.Config.CommandChannelId.Contains(e.Channel.Id.ToString()) && e.Message.Content.StartsWith(prefix))
                        {
                            cmdConditionMatch = true;
                            matchedPrefix = prefix;
                        }
                    }
                    //execute commands
                    if (cmdConditionMatch)
                    {
                        var cmdArgs = e.Message.Content.Substring(matchedPrefix.Length);
                        var cmd = cmdArgs.Split(' ')[0];

                        // Check for permission
                        if (Plugin.Config.CommandPerms.Count() > 0)
                        {
                            var userId = e.Author.Id.ToString();
                            bool hasRolePerm = (await e.Guild.GetMemberAsync(e.Author.Id)).Roles.Where(r => Plugin.Config.CommandPerms.Where(c => c.Player.Equals(r.Id.ToString())).Any()).Any();

                            if (Plugin.Config.CommandPerms.Where(c =>
                            {
                                if (!hasRolePerm && !c.Player.Equals(userId))
                                    return true;
                                else
                                if ((c.Player.Equals(userId) || hasRolePerm) && c.Permission.Equals(cmd) || c.Permission.Equals("*"))
                                    return false;

                                return true;
                            }).Any())
                            {
                                SendCmdResponse($"No permission for command: {cmd}", e.Channel, DiscordColor.Red, cmd);
                                return;
                            }
                        }

                        // Server start command
                        if (cmd.Equals("bridge-startserver"))
                        {
                            if (Plugin.Torch.CurrentSession == null)
                            {
                                Plugin.Torch.Start();
                                SendCmdResponse("Torch initiated!", e.Channel, DiscordColor.Green, cmd);
                            }
                            else
                            {
                                SendCmdResponse("Torch is already running!", e.Channel, DiscordColor.Yellow, cmd);
                            }
                            return;
                        }

                        if (Plugin.Torch.CurrentSession?.State == TorchSessionState.Loaded)
                        {
                            var manager = Plugin.Torch.CurrentSession.Managers.GetManager<CommandManager>();
                            var command = manager.Commands.GetCommand(cmdArgs, out string argText);

                            if (command == null)
                            {
                                SendCmdResponse($"Command not found: {cmdArgs}", e.Channel, DiscordColor.Red, cmd);
                            }
                            else
                            {
                                var cmdPath = string.Join(".", command.Path);
                                var splitArgs = Regex.Matches(argText, "(\"[^\"]+\"|\\S+)").Cast<Match>().Select(x => x.ToString().Replace("\"", "")).ToList();
                                SEDiscordBridgePlugin.Log.Trace($"Invoking {cmdPath} for server.");

                                var context = new SEDBCommandHandler(Plugin.Torch, command.Plugin, Sync.MyId, argText, splitArgs);
                                context.ResponeChannel = e.Channel;
                                context.OnResponse += OnCommandResponse;
                                var invokeSuccess = false;
                                Plugin.Torch.InvokeBlocking(() => invokeSuccess = command.TryInvoke(context));
                                SEDiscordBridgePlugin.Log.Debug($"invokeSuccess {invokeSuccess}");
                                if (!invokeSuccess)
                                {
                                    SendCmdResponse($"Error executing command: {cmdArgs}", e.Channel, DiscordColor.Red, cmd);
                                }
                                SEDiscordBridgePlugin.Log.Info($"Server ran command '{cmdArgs}'");
                            }
                        }
                        else
                        {
                            SendCmdResponse("Error: Server is not running.", e.Channel, DiscordColor.Red, cmd);
                        }
                        return;
                    }
                }

                //send to global
                if (Plugin.Config.ChatChannelId.Contains(e.Channel.Id.ToString()))
                {
                    string sender = Plugin.Config.ServerName;

                    if (!Plugin.Config.AsServer)
                    {
                        if (Plugin.Config.UseNicks)
                            sender = e.Guild.GetMemberAsync(e.Author.Id).Result.Nickname;
                        else
                            sender = e.Author.Username;
                    }

                    var manager = Plugin.Torch.CurrentSession.Managers.GetManager<IChatManagerServer>();
                    var dSender = Plugin.Config.Format2.Replace("{p}", sender);
                    var msg = MentionIDToName(e.Message);
                    lastMessage = dSender + msg;
                    manager.SendMessageAsOther(dSender, msg, Plugin.Config.GlobalColor);
                }

                //send to faction
                var channelIds = Plugin.Config.FactionChannels.Where(c => e.Channel.Id.Equals(c.Channel));
                if (channelIds.Count() > 0)
                {
                    foreach (var chId in channelIds)
                    {
                        IEnumerable<IMyFaction> facs = MySession.Static.Factions.Factions.Values.Where(f => f.Tag.Equals(chId.Faction));
                        if (facs.Count() > 0)
                        {
                            IMyFaction fac = facs.First();
                            foreach (MyFactionMember mb in fac.Members.Values)
                            {
                                if (!MySession.Static.Players.GetOnlinePlayers().Any(p => p.Identity.IdentityId.Equals(mb.PlayerId)))
                                    continue;

                                ulong steamid = MySession.Static.Players.TryGetSteamId(mb.PlayerId);
                                string sender = Plugin.Config.ServerName;
                                if (!Plugin.Config.AsServer)
                                {
                                    if (Plugin.Config.UseNicks)
                                        sender = (await e.Guild.GetMemberAsync(e.Author.Id)).Nickname;
                                    else
                                        sender = e.Author.Username;
                                }
                                var manager = Plugin.Torch.CurrentSession.Managers.GetManager<IChatManagerServer>();
                                var dSender = Plugin.Config.FacFormat2.Replace("{p}", sender);
                                var msg = MentionIDToName(e.Message);
                                lastMessage = dSender + msg;
                                manager.SendMessageAsOther(dSender, msg, Plugin.Config.FacColor, steamid);
                            }
                        }
                    }
                }
            }
            return;
        }

        private void SendCmdResponse(string response, DiscordChannel chann, DiscordColor color, string command)
        {
            if (Plugin.Config.Embed)
            {
                DiscordEmbed discordEmbed = new DiscordEmbedBuilder()
                {
                    Description = response,
                    Color = color,
                    Title = string.IsNullOrEmpty(command) ? null : $"Command: {command}"
                };
                DiscordMessage dms = Discord.SendMessageAsync(chann, "", false, discordEmbed).Result;

                botId = dms.Author.Id;
                if (Plugin.Config.RemoveResponse > 0)
                    Task.Delay(Plugin.Config.RemoveResponse * 1000).ContinueWith(t => dms?.DeleteAsync());
            }
            else
            {
                DiscordMessage dms = Discord.SendMessageAsync(chann, response).Result;
                botId = dms.Author.Id;
                if (Plugin.Config.RemoveResponse > 0)
                    Task.Delay(Plugin.Config.RemoveResponse * 1000).ContinueWith(t => dms?.DeleteAsync());
            }
        }

        private string MentionNameToID(string msg, DiscordChannel chann)
        {
            try
            {
                var parts = msg.Split(' ');
                foreach (string part in parts)
                {
                    if (part.Length > 2)
                    {
                        if (part.StartsWith("@"))
                        {
                            string name = Regex.Replace(part.Substring(1), @"[,#]", "");
                            if (string.Compare(name, "everyone", true) == 0 && !Plugin.Config.MentEveryone)
                            {
                                msg = msg.Replace(part, part.Substring(1));
                                continue;
                            }
                            if (string.Compare(name, "here", true) == 0 && !Plugin.Config.MentEveryone)
                            {
                                msg = msg.Replace(part, part.Substring(1));
                                continue;
                            }
                            try
                            {
                                var members = chann.Guild.GetAllMembersAsync().Result;

                                if (!Plugin.Config.MentOthers)
                                {
                                    continue;
                                }
                                var memberByNickname = members.FirstOrDefault((u) => string.Compare(u.Nickname, name, true) == 0);
                                if (memberByNickname != null)
                                {
                                    msg = msg.Replace(part, $"<@{memberByNickname.Id}>");
                                    continue;
                                }
                                var memberByUsername = members.FirstOrDefault((u) => string.Compare(u.Username, name, true) == 0);
                                if (memberByUsername != null)
                                {
                                    msg = msg.Replace(part, $"<@{memberByUsername.Id}>");
                                    continue;
                                }
                            }
                            catch (Exception)
                            {
                                SEDiscordBridgePlugin.Log.Warn("Error on convert a member id to name on mention other players.");
                                continue;
                            }
                        }

                        var emojis = chann.Guild.Emojis;
                        if (part.StartsWith(":") && part.EndsWith(":") && emojis.Any(e => string.Compare(e.GetDiscordName(), part, true) == 0))
                        {
                            msg = msg.Replace(part, $"<{part}{emojis.Where(e => string.Compare(e.GetDiscordName(), part, true) == 0).First().Id}>");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SEDiscordBridgePlugin.Log.Warn(e, "Error on convert a member id to name on mention other players.");
            }
            return msg;
        }

        private string MentionIDToName(DiscordMessage ddMsg)
        {
            string msg = ddMsg.Content;
            var parts = msg.Split(' ');
            foreach (string part in parts)
            {
                if (part.StartsWith("<@!") && part.EndsWith(">"))
                {
                    try
                    {
                        ulong id = ulong.Parse(part.Substring(3, part.Length - 4));

                        var name = Discord.GetUserAsync(id).Result.Username;
                        if (Plugin.Config.UseNicks)
                            name = ddMsg.Channel.Guild.GetMemberAsync(id).Result.Nickname;

                        msg = msg.Replace(part, "@" + name);
                    }
                    catch (FormatException) { }
                }
                if (part.StartsWith("<:") && part.EndsWith(">"))
                {
                    string id = part.Substring(2, part.Length - 3);
                    msg = msg.Replace(part, ":" + id.Split(':')[0] + ":");
                }
            }
            return msg;
        }

        private void OnCommandResponse(DiscordChannel channel, string message, string sender = "Server", string font = "White")
        {
            SEDiscordBridgePlugin.Log.Debug($"response length {message.Length}");
            if (message.Length > 0)
            {
                message = message.Replace("_", "\\_")
                    .Replace("*", "\\*")
                    .Replace("~", "\\~");
                if (Plugin.Config.StripGPS)
                {
                    message = Regex.Replace(message, @"@?\ ?[0-9E+.-]+,[0-9E+.-]+,[0-9E+.-]+", "", RegexOptions.Multiline);
                }

                const int chunkSize = 2000 - 1; // Remove 1 just ensure everything is ok

                if (message.Length <= chunkSize)
                {
                    SendCmdResponse(message, channel, DiscordColor.Green, null);
                }
                else
                {
                    var index = 0;
                    do
                    {

                        SEDiscordBridgePlugin.Log.Debug($"while iteration index {index}");

                        /* if remaining part of message is small enough then just output it. */
                        if (index + chunkSize >= message.Length)
                        {
                            SendCmdResponse(message.Substring(index), channel, DiscordColor.Green, null);
                            break;
                        }

                        var chunk = message.Substring(index, chunkSize);
                        var newLineIndex = chunk.LastIndexOf("\n");
                        SEDiscordBridgePlugin.Log.Debug($"while iteration newLineIndex {newLineIndex}");

                        SendCmdResponse(chunk.Substring(0, newLineIndex), channel, DiscordColor.Green, null);
                        index += newLineIndex + 1;

                    } while (index < message.Length);
                }
            }
        }
    }
}
