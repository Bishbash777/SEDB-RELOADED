using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static SEDiscordBridge.DiscordBridge;

namespace SEDiscordBridge
{
    [XmlType("DeathRoutes")]
    public class DeathRoutes
    {
        [XmlElement("EntityType")]
        public EntityType EntityType { get; set; }

        [XmlElement("ChannelId")]
        public ulong ChannelId { get; set; }

        public DeathRoutes() { }
        public DeathRoutes(EntityType entityType, ulong channelId)
        {
            EntityType = entityType;
            ChannelId = channelId;
        }
    }
}
