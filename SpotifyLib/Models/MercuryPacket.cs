namespace SpotifyLib.Models
{
    public readonly struct MercuryPacket
    {
        public MercuryPacket(MercuryPacketType cmd, byte[] payload)
        {
            Cmd = cmd;
            Payload = payload;
        }

        public MercuryPacketType Cmd { get; }
        public byte[] Payload { get; }

        public static MercuryPacketType ParseType(byte input)
        {
            return (MercuryPacketType)input;
        }
    }

}
