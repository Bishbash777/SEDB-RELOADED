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
}
