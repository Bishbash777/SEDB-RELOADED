using NLog;
using System.Collections.Generic;
using System.Linq;
using Torch.API;
using System.Net.Http;
using System.Web;
using VRage.Game.ModAPI;
using Sandbox.Game.World;
using System.Threading.Tasks;
using VRage.Game.Entity;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Weapons;
using Sandbox.Game.Entities.Cube;
using VRageMath;
using System.Collections.ObjectModel;
using System.Reflection;
using VRage;

namespace SEDiscordBridge {

    public class utils {
        public static ITorchBase Torch { get; }
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static string GetSubstringByString(string from, string until, string wholestring) {
            return wholestring.Substring((wholestring.IndexOf(from) + from.Length), (wholestring.IndexOf(until) - wholestring.IndexOf(from) - from.Length));
        }

        public static MyEntity EntityByIdOrDefault<T>(long id) where T : MyEntity
        {
            MyEntities.TryGetEntityById(id, out T result);
            return result;
        } 

        public static string GetEntityOwnerName(object entity)
        {
            if (entity == null) return "Nobody";
            else if (entity is MyCharacter character) return character.CustomNameWithFaction.ToString();
            else if (entity is MyCubeBlock cubeBlock) return cubeBlock.BuiltBy == 0 ? "Nobody" : GetPlayerByNameOrId(cubeBlock.BuiltBy.ToString()).DisplayName;
            else if (entity is MySlimBlock block) return block.BuiltBy == 0 ? "Nobody" : GetPlayerByNameOrId(block.BuiltBy.ToString()).DisplayName;
            else if (entity is MyCubeGrid cubeGrid) return GetPlayerByNameOrId(cubeGrid.GetBiggestGridInGroup().BigOwners.FirstOrDefault().ToString())?.DisplayName ?? "Nobody";
            else if (entity is MyEngineerToolBase toolBase) return GetPlayerByNameOrId(toolBase.OwnerId.ToString())?.DisplayName ?? "Nobody";
            else if (entity is MyAutomaticRifleGun rifleGun) return GetPlayerByNameOrId(rifleGun.OwnerId.ToString())?.DisplayName ?? "Nobody";

            return "Nobody";
        }

        public static MyPlayer GetEntityOwner(object entity)
        {
            if (entity == null) return default;
            else if (entity is MyCharacter character) return GetPlayerByNameOrId(character.GetPlayerIdentityId().ToString());
            else if (entity is MyCubeBlock cubeBlock) return cubeBlock.BuiltBy == 0 ? default : GetPlayerByNameOrId(cubeBlock.BuiltBy.ToString());
            else if (entity is MySlimBlock block) return block.BuiltBy == 0 ? default : GetPlayerByNameOrId(block.BuiltBy.ToString());
            else if (entity is MyCubeGrid cubeGrid) return GetPlayerByNameOrId(cubeGrid.GetBiggestGridInGroup().BigOwners.FirstOrDefault().ToString());
            else if (entity is MyEngineerToolBase toolBase) return GetPlayerByNameOrId(toolBase.OwnerId.ToString());
            else if (entity is MyAutomaticRifleGun rifleGun) return GetPlayerByNameOrId(rifleGun.OwnerId.ToString());

            return default;
        }

        public static string GetEntityName(object entity)
        {
            if (entity == null) return "Unknown";
            else if (entity is MyCharacter character) return character.CustomNameWithFaction.ToString();
            else if (entity is MyCubeBlock cubeBlock) return cubeBlock.CubeGrid?.DisplayName ?? "Unknown";
            else if (entity is MySlimBlock block) return block.CubeGrid?.DisplayName ?? "Unknown";
            else if (entity is MyCubeGrid cubeGrid) return cubeGrid?.DisplayName ?? "Unknown";
            else if (entity is MyEntity myEntity) return myEntity.DisplayName;

            return "Unknown";
        }

        public static MyEntity GetEntityFromObject(object obj)
        {
            if (obj is MyEntity entity) return entity;
            if (obj is MySlimBlock slimBlock) return slimBlock.FatBlock;
            return default;
        }

        public static Dictionary<string, string> ParseQueryString(string queryString) {
            var nvc = HttpUtility.ParseQueryString(queryString);
            return nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
        }

        public static MyPlayer GetPlayerByNameOrId(string nameOrPlayerId) {
            if (!long.TryParse(nameOrPlayerId, out long id)) {
                foreach (var identity in MySession.Static.Players.GetAllIdentities()) {
                    if (identity.DisplayName == nameOrPlayerId) {
                        id = identity.IdentityId;
                    }
                }
            }

            if (MySession.Static.Players.TryGetPlayerId(id, out MyPlayer.PlayerId playerId)) {
                if (MySession.Static.Players.TryGetPlayerById(playerId, out MyPlayer player)) {
                    return player;
                }
            }

            return null;
        }


        public static async Task<string> dataRequest(string uSteamid, string guid, string funciton) {
            HttpResponseMessage response;
            using (HttpClient clients = new HttpClient()) {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>
                {
                        new KeyValuePair<string, string>("DATA", uSteamid),
                        new KeyValuePair<string, string>("FUNCTION",funciton),
                        new KeyValuePair<string, string>("API_KEY","TEST_KEY"),
                        new KeyValuePair<string, string>("GUID", guid),
                };
                FormUrlEncodedContent content = new FormUrlEncodedContent(pairs);
                response = await clients.PostAsync("http://sedb.uk/api/index.php", content);
            }
            return await response.Content.ReadAsStringAsync();
        }

        public static ReadOnlyObservableCollection<ColorStruct> Colors { get; } = new ReadOnlyObservableCollection<ColorStruct>(new ObservableCollection<ColorStruct>(typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(b => new ColorStruct((Color)b.GetValue(null), b.Name))));
        public static ColorStruct GetFromColor(Color color) => Colors.ToList().Find(b => b.Color == color);
    }

    public struct ColorStruct
    {
        public ColorStruct (Color color, string name) { Color = color; DisplayName = name; }
        public string DisplayName { get; set; }
        public Color Color { get; set; }
    }
}
