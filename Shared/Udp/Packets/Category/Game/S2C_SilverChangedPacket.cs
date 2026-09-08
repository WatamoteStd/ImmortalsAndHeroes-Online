
using System.Buffers.Binary;
using Shared.Udp.Interfaces;

namespace Shared.Udp.Packets.Category.Game;

public struct S2C_SilverChangedPacket : INetworkPacket
{
    
    public int Length {get; private set;}
    public long SilverValue;
    public long TotalSilver;

    public int Serialize(Span<byte> buffer)
    {
        
        Length = 0;
        BinaryPrimitives.WriteInt64LittleEndian(buffer, SilverValue);
        Length += 8;
        BinaryPrimitives.WriteInt64LittleEndian(buffer[Length..], TotalSilver);
        Length += 8;

        return Length;

    }
    public void Deserialize(ReadOnlySpan<byte> buffer)
    {
        
        Length = 0;
        SilverValue = BinaryPrimitives.ReadInt64LittleEndian(buffer);
        Length += 8;
        TotalSilver = BinaryPrimitives.ReadInt64LittleEndian(buffer[Length..]);
        Length += 8;

    }

}