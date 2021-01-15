using System;
using System.Text;
using SpotifyLibV2.Mercury;

namespace SpotifyLibV2.Exceptions
{
    public class MercuryUnknownCmdException : Exception
    {
        public MercuryUnknownCmdException(MercuryPacket packet) : base("Unknown CMD 0x" + packet.Cmd)
        {
            Packet = packet;
        }
        public MercuryPacket Packet { get; }

        public string TryReadData(out string output)
        {
            output = Encoding.UTF8.GetString(Packet.Payload);
            return output;
        }
    }
}
