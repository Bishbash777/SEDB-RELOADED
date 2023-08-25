using NLog;
using System.Collections.Generic;
using System.Linq;
using Torch.API;
using System.Net.Http;
using System.Web;
using Sandbox.Game.World;
using System.Threading.Tasks;
using System.Reflection;

namespace SEDiscordBridge {

    public class Utils {
        public static ITorchBase Torch { get; }
        public static bool debug = true;
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static string GetSubstringByString(string from, string until, string wholestring) {
            return wholestring.Substring((wholestring.IndexOf(from) + from.Length), (wholestring.IndexOf(until) - wholestring.IndexOf(from) - from.Length));
        }

        public static Dictionary<string, string> ParseQueryString(string queryString) {
            var nvc = HttpUtility.ParseQueryString(queryString);
            return nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
        }

        public static MyPlayer GetPlayerByNameOrId(string nameOrPlayerId)
        {
            if (!long.TryParse(nameOrPlayerId, out long id))
            {
                foreach (var identity in MySession.Static.Players.GetAllIdentities())
                {
                    if (identity.DisplayName == nameOrPlayerId)
                    {
                        id = identity.IdentityId;
                    }
                }
            }

            if (MySession.Static.Players.TryGetPlayerId(id, out MyPlayer.PlayerId playerId))
            {
                if (MySession.Static.Players.TryGetPlayerById(playerId, out MyPlayer player))
                {
                    return player;
                }
            }

            return null;
        }

        public static MethodInfo FindOverLoadMethod(MethodInfo[] methodInfo, string name, int parameterLenth) {
            MethodInfo method = null;
            foreach (var DecalredMethod in methodInfo) {
                if (debug)
                    Log.Info($"Method name: {DecalredMethod.Name}");
                if (DecalredMethod.GetParameters().Length == parameterLenth && DecalredMethod.Name == name) {
                    method = DecalredMethod;
                    break;
                }
            }
            return method;
        }
    }
}
