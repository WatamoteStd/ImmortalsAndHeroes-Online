using Shared.Udp.Interfaces;

namespace Shared.Udp.Packets.Category.Market;

public struct C2S_RoyalContractInfoRequestPacket : INetworkPacket
{
    
    public int Length {get; private set;}
    public byte DummyByte;

    public int Serialize(Span<byte> buffer)
    {
        
        Length = 0;
        DummyByte = 1;

        buffer[Length++] = DummyByte;
        Length++;

        return Length;

    }
    public void Deserialize(ReadOnlySpan<byte> buffer)
    {
        
        Length = 0;
        DummyByte = buffer[Length];
        Length = 1;

    }

}