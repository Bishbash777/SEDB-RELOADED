using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRage.Game.Entity;
using VRage.Game.ModAPI.Interfaces;
using VRageMath;

namespace SEDiscordBridge
{
    public enum SendTarget { Discord, Ingame, All }

    [XmlInclude(typeof(StringInvalid))]
    [XmlType("DamageTexts")]
    public class DamageTexts
    {

        public DamageType DamageType { get; set; }

        public SendTarget SendTarget { get; set; }

        public ObservableCollection<StringInvalid> Strings { get; set; }

        public DamageTexts() {  }
        public DamageTexts(DamageType damageType, ObservableCollection<StringInvalid> strings)
        {
            DamageType = damageType;
            Strings = strings;
        }
        public static string Formate(DiscordBridge.DeathMessage message, int count = 1)
        {
            var text = (message.TargetType == DiscordBridge.EntityType.Character ? SEDiscordBridgePlugin.Static.Config.PlayerDeathMessages : SEDiscordBridgePlugin.Static.Config.GridDeathMessages).ToList().Find(b => b.DamageType == message.DamageType);
            if (text == null)
                return "";
            var str = text.Strings.PickRandom().Value
                            .Replace("{target}", utils.GetEntityName(message.Target))
                            .Replace("{targetOwner}", message.TargetOwner.DisplayName)
                            .Replace("{attacker}", utils.GetEntityName(message.Attacker))
                            .Replace("{attackerOwner}", message.AttackerOwner?.DisplayName ?? "Nobody")
                            .Replace("{type}", message.DamageType.ToString())
                            .Replace("{count}", count.ToString())
                            .Replace("{ts}", TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToString());
            if (text.SendTarget == SendTarget.Discord)
                return str;
            else if (text.SendTarget == SendTarget.Ingame)
                SEDiscordBridgePlugin.Static.ChatManager.SendMessageAsOther("DeathLog", str, Color.DarkGoldenrod);
            else if (text.SendTarget == SendTarget.All)
            {
                SEDiscordBridgePlugin.Static.ChatManager.SendMessageAsOther("DeathLog", str, Color.DarkGoldenrod);
                return str;
            }

            return "";
        }
    }
}
