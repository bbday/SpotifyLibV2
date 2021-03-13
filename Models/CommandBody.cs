using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Encoders;

namespace SpotifyLibV2.Models
{
    public class CommandBody
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

        public CommandBody([NotNull] JObject obj)
        {
            this.Obj = obj;

            if (obj.ContainsKey("data")) Data = Base64.Decode(obj["data"].ToString());
            else Data = null;

            if (obj.ContainsKey("value")) Value = obj["value"].ToString();
            else Value = null;
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
