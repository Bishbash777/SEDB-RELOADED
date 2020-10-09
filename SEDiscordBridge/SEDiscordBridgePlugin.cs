﻿using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Gui;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
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
        
        private UserControl _control;
        private TorchSessionManager _sessionManager;
        private ChatManagerServer _chatmanager;
        public IChatManagerServer ChatManager => _chatmanager ?? (Torch.CurrentSession.Managers.GetManager<IChatManagerServer>());
        private IMultiplayerManagerBase _multibase;
        private Timer _timer;
        private TorchServer torchServer;
        private readonly HashSet<ulong>_conecting = new HashSet<ulong>();

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static SEDiscordBridgePlugin Static { get; private set; }

        /// <inheritdoc />
        public UserControl GetControl() => _control ?? (_control = new SEDBControl(this));

        public void Save() => _config?.Save();


        /// <inheritdoc />
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            torchServer = (TorchServer)torch;
            Static = this;
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
            try
            {
                if (!Config.Enabled) return;


                if (msg.AuthorSteamId != null && !ChatManager.MutedUsers.Contains((ulong)msg.AuthorSteamId))
                {
                    switch (msg.Channel)
                    {
                        case ChatChannel.Global:
                            DDBridge.RunSendChatTask(msg.Author, msg.Message);
                            break;
                        case ChatChannel.GlobalScripted:
                            DDBridge.RunSendChatTask(msg.Author, msg.Message);
                            break;
                        case ChatChannel.Faction:
                            IMyFaction fac = MySession.Static.Factions.TryGetFactionById(msg.Target);
                            DDBridge.RunSendFacTask(msg.Author, msg.Message, fac.Name);
                            break;
                    }
                }
                else if (Config.ServerToDiscord && msg.Channel.Equals(ChatChannel.Global) && !msg.Message.StartsWith(Config.CommandPrefix) && msg.Target.Equals(0))
                {
                    DDBridge.RunSendChatTask(msg.Author, msg.Message);
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
                    DamageSystem.Init();
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

        public void LoadSEDB()
        {
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
                        DiscordBridge.CooldownNeutral += (60/DiscordBridge.Factor);
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
                Task.Run(() =>
                {
                    System.Threading.Thread.Sleep(1000);
                    if (_conecting.Contains(character.ControlSteamId) && character.IsPlayer && Config.Join.Length > 0)
                    {
                        DDBridge.SendStatusMessage(character.DisplayName, Config.Join);
                        //After spawn on world, remove from connecting list
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
