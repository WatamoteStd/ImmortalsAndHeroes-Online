using System.Buffers.Binary;
using Shared.Udp.Interfaces;

namespace Shared.Udp.Packets.Category.Settlement;

public struct S2C_TreasureUpdatePacket : INetworkPacket
{
    
    public int Length {get; private set;}
    public ulong Amount;
    public bool Success;

    public int Serialize(Span<byte> buffer)
    {
        Length = 0;
        
        buffer[Length] = (byte)(Success ? 1 : 0);
        Length++;

        BinaryPrimitives.WriteUInt64LittleEndian(buffer[Length..], Amount);
        Length += 8;

        return Length;
    }

    public void Deserialize(ReadOnlySpan<byte> buffer)
    {
        Length = 0;

        Success = buffer[Length] != 0;
        Length++;

        Amount = BinaryPrimitives.ReadUInt64LittleEndian(buffer[Length..]);
        Length += 8;
    }
}


