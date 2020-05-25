using NLog;
using System.Net.Http;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;
using System.Collections.Generic;
using System.Web;
using Sandbox.Game;
using System.Diagnostics;
namespace SEDiscordBridge
{

    public class Commands : CommandModule
    {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public SEDiscordBridgePlugin Plugin => (SEDiscordBridgePlugin)Context.Plugin;

        [Command("bridge-reload", "Reload current SEDB configuration")]
        [Permission(MyPromoteLevel.Admin)]
        public void ReloadBridge()
        {
            Plugin.InitConfig();
            Plugin.DDBridge?.SendStatus(null);

            if (Plugin.Config.Enabled)
            {
                if (Plugin.Torch.CurrentSession == null && !Plugin.Config.PreLoad)
                {
                    Plugin.UnloadSEDB();

                }
                else
                {
                    Plugin.LoadSEDB();
                }
            }
            else
            {
                Plugin.UnloadSEDB();
            }
            Context.Respond("SEDB plugin reloaded!");
        }

        [Command("sedb-link", "Link you steamID to a discord account")]
        [Permission(MyPromoteLevel.None)]
        public async void link() {
            HttpResponseMessage response;
            using (HttpClient clients = new HttpClient()) {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>
                {
                        new KeyValuePair<string, string>("steamid",Context.Player.SteamUserId.ToString()),
                };
                FormUrlEncodedContent content = new FormUrlEncodedContent(pairs);
                response = await clients.PostAsync("http://sedb.uk/discord/guid-manager.php", content);
            }
            string texts = await response.Content.ReadAsStringAsync();
            utils utils = new utils();
            Dictionary<string,string> kvp = utils.ParseQueryString(texts);
            if (kvp["existance"] == "false") {
                MyVisualScriptLogicProvider.OpenSteamOverlay($"https://steamcommunity.com/linkfilter/?url=http://sedb.uk/?guid={kvp["guid"]}&steamid={Context.Player.SteamUserId}", Context.Player.IdentityId);
            }
            else {
                Context.Respond("Your SteamId has already been linked to discord, if this has not been authenticated by yourself... Please contact your admin");
            }
            
        }

        [Command("bridge-enable", "To enable SEDB if disabled")]
        [Permission(MyPromoteLevel.Admin)]
        public void EnableBridge()
        {
            Plugin.LoadSEDB();
            Context.Respond("SEDB plugin enabled!");
        }

        [Command("bridge-disable", "To disable SEDB if enabled")]
        [Permission(MyPromoteLevel.Admin)]
        public void DisableBridge()
        {
            Plugin.UnloadSEDB();
            Context.Respond("SEDB plugin disabled!");
        }
    }
}
