using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Encoders;

namespace SpotifyLib.Models
{
    public readonly struct CommandBody
    {
        public JObject Obj
        {
            get;
        }
        public byte[] Data
        {
            get;
        }
        public string Value
        {
            get;
        }

        public CommandBody(JObject obj)
        {
            Obj = obj;
            Data = obj.ContainsKey("data") ? Base64.Decode(obj["data"].ToString()) : null;
            Value = obj.ContainsKey("value") ? obj["value"].ToString() : null;
        }


        public int? ValueInt()
        {
            return Value == null ? null : int.Parse(Value);
        }

        public bool? ValueBool()
        {
            return Value == null ? null : bool.Parse(Value);
        }
    }
}
