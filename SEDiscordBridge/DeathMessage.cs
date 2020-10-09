using Sandbox.Game.World;
using VRage.Game.Entity;

namespace SEDiscordBridge
{
    public partial class DiscordBridge
    {
        public struct DeathMessage
        {
            public MyEntity Attacker;
            public MyPlayer AttackerOwner;
            public DamageType DamageType;
            public MyEntity Target;
            public MyPlayer TargetOwner;
            public EntityType TargetType;
        }
        public enum EntityType { Character, Grid }
    }
}
