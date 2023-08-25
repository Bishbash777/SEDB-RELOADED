using NLog;
using System.Net.Http;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;
using System.Collections.Generic;
using Sandbox.Game;
using Sandbox.Game.World;
using System.Threading;
using DSharpPlus.Entities;

namespace SEDiscordBridge
{
    [Category("sedb")]
    public class Commands : CommandModule {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public SEDiscordBridgePlugin Plugin => (SEDiscordBridgePlugin)Context.Plugin;

        [Command("reload", "Reload SEDB Service")]
        [Permission(MyPromoteLevel.Admin)]
        public void ReloadBridge()
        {
            if (Plugin.Config.Enabled) {
                Plugin.UnloadSEDB();
                Thread.Sleep(100);
                Plugin.LoadSEDB();
                Context.Respond("SEDB plugin reloaded!");
            }
            else
                Context.Respond("SEDB plugin Disabled!");
        }

        [Command("reloadconfig", "Reload current SEDB configuration")]
        [Permission(MyPromoteLevel.Admin)]
        public void ReloadBridgeConfig() {
            Plugin.InitConfig();
            Plugin.DDBridge?.SendStatus(null, UserStatus.DoNotDisturb);

            if (Plugin.Config.Enabled) {
                if (Plugin.Torch.CurrentSession == null && !Plugin.Config.PreLoad) {
                    Plugin.UnloadSEDB();

                }
                else {
                    Plugin.LoadSEDB();
                }
            }
            else {
                Plugin.UnloadSEDB();
            }
            Context.Respond("SEDB plugin reloaded!");
        }
        
        [Command("enable", "To enable SEDB if disabled")]
        [Permission(MyPromoteLevel.Admin)]
        public void EnableBridge()
        {
            Plugin.LoadSEDB();
            Context.Respond("SEDB plugin enabled!");
        }

        [Command("disable", "To disable SEDB if enabled")]
        [Permission(MyPromoteLevel.Admin)]
        public void DisableBridge()
        {
            Plugin.UnloadSEDB();
            Context.Respond("SEDB plugin disabled!");
        }
    }
}
