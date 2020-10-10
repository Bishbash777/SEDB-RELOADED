using System.Xml.Serialization;
using Torch;

namespace SEDiscordBridge
{
    [XmlType("StringInvalid")]
    public class StringInvalid : ViewModel
    {
        public StringInvalid() { }
        public StringInvalid(string value) 
        { 
            Value = value; 
        }
        private string _value = "";
        public string Value { get => _value; set => SetValue(x => _value = x, value); } 
    }

    [XmlType("FactionChannel")]
    public class FactionChannel : ViewModel
    {
        private string _faction;
        private ulong _channel;

        public string Faction { get => _faction; set => SetValue(ref _faction, value); }
        public ulong Channel { get => _channel; set => SetValue(ref _channel, value); }
    }

    [XmlType("CommandPermission")]
    public class CommandPermission : ViewModel
    {
        private ulong _player;
        private string _permission;

        public ulong Player { get => _player; set => SetValue(ref _player, value); }
        public string Permission { get => _permission; set => SetValue(ref _permission, value); }
    }
}
