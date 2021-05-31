namespace Spotify.Lib.Models
{
    public readonly struct MercuryPacket
    {
        internal readonly MercuryPacketType Cmd;
        internal readonly byte[] Payload;

        internal MercuryPacket(MercuryPacketType cmd, byte[] payload)
        {
            Cmd = cmd;
            Payload = payload;
        }

        public static MercuryPacketType ParseType(byte input)
        {
            return (MercuryPacketType) input;
        }
    }
}