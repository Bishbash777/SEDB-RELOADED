using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Gui;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Managers;
using Torch.Managers.ChatManager;
using Torch.Server;
using Torch.Session;
using VRage.Game.ModAPI;

namespace SEDiscordBridge
{
    public sealed class SEDiscordBridgePlugin : TorchPluginBase, IWpfPlugin
    {
        public SEDBConfig Config => _config?.Data;

        public Persistent<SEDBConfig> _config;

        public DiscordBridge DDBridge;
        public MethodInfo InjectDiscordIDMethod = null;

        private UserControl _control;
        private TorchSessionManager _sessionManager;
        private ChatManagerServer _chatmanager;
        private IChatManagerServer ChatManager => _chatmanager ?? (Torch.CurrentSession.Managers.GetManager<IChatManagerServer>());
        private IMultiplayerManagerBase _multibase;
        private List<ulong> messageQueue = new List<ulong>();
        private Timer _timer;
        private TorchServer torchServer;
        private readonly HashSet<ulong> _conecting = new HashSet<ulong>();

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <inheritdoc />
        public UserControl GetControl() => _control ?? (_control = new SEDBControl(this));

        public void Save() => _config?.Save();


        /// <inheritdoc />
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            torchServer = (TorchServer)torch;

            //Init config
            InitConfig();

            //pre-load
            if (Config.Enabled) LoadSEDB();
        }

        public void InitConfig()
        {
            try
            {
                _config = Persistent<SEDBConfig>.Load(Path.Combine(StoragePath, "SEDiscordBridge.cfg"));
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }
            if (_config?.Data == null)
                _config = new Persistent<SEDBConfig>(Path.Combine(StoragePath, "SEDiscordBridge.cfg"), new SEDBConfig());
        }

        private void MessageRecieved(TorchChatMessage msg, ref bool consumed)
        {
            Task.Run(async () => SendAsync(msg));
        }

        private async void SendAsync(TorchChatMessage msg)
        {
            try
            {
                if (!Config.Enabled) return;


                if (msg.AuthorSteamId != null && !ChatManager.MutedUsers.Contains((ulong)msg.AuthorSteamId))
                {
                    switch (msg.Channel)
                    {
                        case ChatChannel.Global:
                            DDBridge.SendChatMessage(msg.Author, msg.Message);
                            break;
                        case ChatChannel.GlobalScripted:
                            DDBridge.SendChatMessage(msg.Author, msg.Message);
                            break;
                        case ChatChannel.Faction:
                            IMyFaction fac = MySession.Static.Factions.TryGetFactionById(msg.Target);
                            DDBridge.SendFacChatMessage(msg.Author, msg.Message, fac.Name);
                            break;
                    }
                }
                else if (Config.ServerToDiscord && msg.Channel.Equals(ChatChannel.Global) && !msg.Message.StartsWith(Config.CommandPrefix) && msg.Target.Equals(0))
                {
                    DDBridge.SendChatMessage(msg.Author, msg.Message);
                    DDBridge.SendFacChatMessage(msg.Author, msg.Message, fac.Name);
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
        }

        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {
            if (!Config.Enabled) return;

            switch (state)
            {
                case TorchSessionState.Loaded:

                    //load
                    LoadSEDB();
                    if (DDBridge != null) DDBridge.SendStatusMessage(null, Config.Started);

                    break;

                case TorchSessionState.Unloading:
                    if (Config.Stopped.Length > 0)
                        DDBridge.SendStatusMessage(null, Config.Stopped);
                    break;

                case TorchSessionState.Unloaded:
                    //unload
                    UnloadSEDB();

                    break;
                default:
                    // ignore
                    break;
            }
        }

        public void UnloadSEDB()
        {
            if (DDBridge != null)
            {
                Log.Info("Unloading Discord Bridge!");
                DDBridge.Stopdiscord();
                DDBridge = null;
                Log.Info("Discord Bridge Unloaded!");
            }
            Dispose();
        }


        public void ReflectEssentials()
        {
            var pluginId = new Guid("cbfdd6ab-4cda-4544-a201-f73efa3d46c0");
            var pluginManager = Torch.Managers.GetManager<PluginManager>();

            if (pluginManager.Plugins.TryGetValue(pluginId, out ITorchPlugin EssentialsPlugin))
            {
                try
                {
                    MethodInfo[] methods = null;
                    methods = EssentialsPlugin.GetType().GetMethods();
                    foreach (var meth in methods)
                    {
                        if (meth.Name == "InsertDiscordID")
                        {
                            InjectDiscordIDMethod = meth;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Warn(e, "failure");
                }

            }
            else
            {
                Log.Info("Essentials Plugin not found! ");
            }
        }

        public void InjectDiscordID(IPlayer player)
        {

            try
            {
                if (InjectDiscordIDMethod != null)
                {
                    string discord_Id = Task.Run(async () => await GetID(player.SteamId)).Result;
                    if (discord_Id != null)
                    {
                        var roledata = DDBridge.GetRoles(ulong.Parse(discord_Id));
                        string discordName = DDBridge.GetName(ulong.Parse(discord_Id));
                        Log.Info($"DiscordID for {player.Name} found! Retrieving role data and injecting into essentials...");
                        InjectDiscordIDMethod.Invoke(null, new object[] { player.SteamId, discord_Id, discordName, roledata });
                    }
                    else if (!messageQueue.Contains(player.SteamId))
                    {
                        messageQueue.Add(player.SteamId);
                    }
                }
                else
                {
                    Log.Warn("Commincation to target method failed!");
                }

            }
            catch (Exception e)
            {
                Log.Warn(e, "failure");
            }


        }

        public async Task<string> GetID(ulong steamid)
        {
            try
            {
                Dictionary<string, string> kvp = utils.ParseQueryString(await utils.dataRequest(steamid.ToString(), Id.ToString(), "get_discord_id"));
                if (kvp["error_code"] == "0")
                {
                    return kvp["data"];
                }
                if (kvp["error_code"] == "1")
                {

                    Log.Warn(kvp["error_message"]);
                }
                if (kvp["error_code"] == "2")
                {
                    Log.Warn("Unauthorised attempt to access data - Contact Bishbash777");
                }
                if (kvp["error_code"] == "3")
                {
                    Log.Warn(kvp["error_message"]);
                }
                return null;
            }
            catch (System.Exception e)
            {
                Log.Warn(e.ToString());
                return null;
            }
        }

        public void LoadSEDB()
        {
            ReflectEssentials();
            if (Config.BotToken.Length <= 0)
            {
                Log.Error("No BOT token set, plugin will not work at all! Add your bot TOKEN, save and restart torch.");
                return;
            }

            if (_sessionManager == null)
            {
                _sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
                if (_sessionManager == null)
                {
                    Log.Warn("No session manager loaded!");
                }
                else
                {
                    _sessionManager.SessionStateChanged += SessionChanged;
                }
            }

            if (Torch.CurrentSession != null)
            {
                if (_multibase == null)
                {
                    _multibase = Torch.CurrentSession.Managers.GetManager<IMultiplayerManagerBase>();
                    if (_multibase == null)
                    {
                        Log.Warn("No join/leave manager loaded!");
                    }
                    else
                    {
                        _multibase.PlayerJoined += _multibase_PlayerJoined;
                        _multibase.PlayerLeft += _multibase_PlayerLeft;
                        MyEntities.OnEntityAdd += MyEntities_OnEntityAdd;
                    }
                }

                if (_chatmanager == null)
                {
                    _chatmanager = Torch.CurrentSession.Managers.GetManager<ChatManagerServer>();
                    if (_chatmanager == null)
                    {
                        Log.Warn("No chat manager loaded!");
                    }
                    else
                    {
                        _chatmanager.MessageRecieved += MessageRecieved;
                    }
                }
                InitPost();
            }
            else if (Config.PreLoad)
            {
                InitPost();
            }
        }

        private void InitPost()
        {
            Log.Info("Starting Discord Bridge!");
            if (DDBridge == null)
                DDBridge = new DiscordBridge(this);

            //send status
            if (Config.UseStatus)
                StartTimer();
        }

        public void StartTimer()
        {
            if (_timer != null) StopTimer();

            _timer = new Timer(Config.StatusInterval);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;
        }

        public void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Elapsed -= _timer_Elapsed;
                _timer.Enabled = false;
                _timer.Dispose();
                _timer = null;
            }
        }

        // for counter within _timer_elapsed() 
        private int i = 0;
        private DateTime timerStart = new DateTime(0);

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!Config.Enabled || DDBridge == null) return;

            if (Torch.CurrentSession == null)
            {
                DDBridge.SendStatus(Config.StatusPre);
            }
            else
            {
                if (timerStart.Ticks == 0) timerStart = e.SignalTime;

                string status = Config.Status;
                DateTime upTime = new DateTime(e.SignalTime.Subtract(timerStart).Ticks);

                Regex regex = new Regex(@"{uptime@(.*?)}");
                if (regex.IsMatch(status))
                {
                    var match = regex.Match(status);
                    string format = match.Groups[0].ToString().Replace("{uptime@", "").Replace("}", "");
                    status = Regex.Replace(status, "{uptime@(.*?)}", upTime.ToString(format));
                }

                DDBridge.SendStatus(status
                .Replace("{p}", MySession.Static.Players.GetOnlinePlayers().Where(p => p.IsRealPlayer).Count().ToString())
                .Replace("{mp}", MySession.Static.MaxPlayers.ToString())
                .Replace("{mc}", MySession.Static.Mods.Count.ToString())
                .Replace("{ss}", torchServer.SimulationRatio.ToString("0.00")));

                if (Config.SimPing)
                {
                    if (torchServer.SimulationRatio < float.Parse(Config.SimThresh))
                    {
                        //condition
                        if (i == DiscordBridge.MinIncrement && DiscordBridge.Locked != 1 && MySession.Static.Players.GetOnlinePlayerCount() > 0)
                        {
                            Task.Run(() => DDBridge.SendSimMessage(Config.SimMessage));
                            i = 0;
                            DiscordBridge.Locked = 1;
                            DiscordBridge.FirstWarning = 1;
                            DiscordBridge.CooldownNeutral = 0;
                            Log.Warn("Simulation warning sent!");
                        }
                        if (DiscordBridge.FirstWarning == 1 && DiscordBridge.CooldownNeutral.ToString("00") == "60" && MySession.Static.Players.GetOnlinePlayerCount() > 0)
                        {
                            Task.Run(() => DDBridge.SendSimMessage(Config.SimMessage));
                            Log.Warn("Simulation warning sent!");
                            DiscordBridge.CooldownNeutral = 0;
                            i = 0;

                        }
                        DiscordBridge.CooldownNeutral += (60 / DiscordBridge.Factor);
                        i++;
                    }
                    else
                    {
                        //reset counter whenever Sim speed warning threshold is not met meaning that sim speed has to stay below
                        //the set threshold for a consecutive minuete to trigger warning
                        i = 0;
                        DiscordBridge.CooldownNeutral = 0;
                    }
                }
            }
        }

        private async void _multibase_PlayerLeft(IPlayer obj)
        {
            if (!Config.Enabled) return;

            //Remove to conecting list
            _conecting.Remove(obj.SteamId);
            if (Config.Leave.Length > 0)
            {
                await Task.Run(() => DDBridge.SendStatusMessage(obj.Name, Config.Leave, obj));
            }
        }

        private async void _multibase_PlayerJoined(IPlayer obj)
        {
            InjectDiscordID(obj);
            if (!Config.Enabled) return;

            //Add to conecting list
            _conecting.Add(obj.SteamId);
            if (Config.Connect.Length > 0)
            {
                await Task.Run(() => DDBridge.SendStatusMessage(obj.Name, Config.Connect, obj));
            }
        }

        private void MyEntities_OnEntityAdd(VRage.Game.Entity.MyEntity obj)
        {
            if (!Config.Enabled) return;

            if (obj is MyCharacter character)
            {
                var manager = Torch.CurrentSession.Managers.GetManager<IChatManagerServer>();
                Task.Run(() =>
                {
                    System.Threading.Thread.Sleep(1000);
                    if (_conecting.Contains(character.ControlSteamId) && character.IsPlayer && Config.Join.Length > 0)
                    {
                        DDBridge.SendStatusMessage(character.DisplayName, Config.Join);
                        //After spawn on world, remove from connecting list
                        if (messageQueue.Contains(character.ControlSteamId))
                        {
                            manager.SendMessageAsOther(null, "Did you know you can link your steamID to your Discord account? Enter '!sedb link' to get started!", VRageMath.Color.Yellow, character.ControlSteamId);
                            messageQueue.Remove(character.ControlSteamId);
                        }
                        _conecting.Remove(character.ControlSteamId);
                    }
                });
            }
        }

        /// <inheritdoc />        
        public override void Dispose()
        {
            if (_multibase != null)
            {
                _multibase.PlayerJoined -= _multibase_PlayerJoined;
                MyEntities.OnEntityAdd -= MyEntities_OnEntityAdd;
                _multibase.PlayerLeft -= _multibase_PlayerLeft;
            }
            _multibase = null;

            if (_sessionManager != null)
                _sessionManager.SessionStateChanged -= SessionChanged;
            _sessionManager = null;

            if (_chatmanager != null)
                _chatmanager.MessageRecieved -= MessageRecieved;
            _chatmanager = null;

            StopTimer();
        }
    }
}
