using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Interfaces;

namespace SEDiscordBridge
{
    public static class DamageSystem
    {
        private static void AfterDamageHandler(object target, MyDamageInformation info)
        {
            if (target != null && target is IMyDestroyableObject destroyableObject)
            {
                if (destroyableObject.Integrity <= 0)
                {
                    DiscordBridge.EntityType type;
                    if (destroyableObject is MyCharacter)
                        type = DiscordBridge.EntityType.Character;
                    else if (destroyableObject is MySlimBlock)
                        type = DiscordBridge.EntityType.Grid;
                    else return;

                    var attacker = utils.EntityByIdOrDefault<MyEntity>(info.AttackerId);

                    SEDiscordBridgePlugin.Static.DDBridge.CreateDeathMessage(attacker,
                                                                             (DamageType)Enum.Parse(typeof(DamageType), info.Type.String, true),
                                                                             utils.GetEntityFromObject(target),
                                                                             type,
                                                                             utils.GetEntityOwner(attacker),
                                                                             utils.GetEntityOwner(destroyableObject));
                }
            }
        }

        public static void Init() => MyDamageSystem.Static.RegisterAfterDamageHandler(int.MaxValue - 3, AfterDamageHandler);
    }
}
