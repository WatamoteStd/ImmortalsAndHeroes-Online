using System.Buffers.Binary;
using Shared.Items;
using Shared.Udp.Interfaces;

namespace Shared.Udp.Packets.Category.Market;

public struct S2C_RoyalContractInfoResponsePacket : INetworkPacket
{
    
    public int Length {get; private set;}
    public ItemType ItemId;
    public uint Count;
    public ulong PricePerUnit;
    public float TaxPercent;
    public uint PerPlayerLimitCount;

    public int Serialize(Span<byte> buffer)
    {
        
        Length = 0;

        BinaryPrimitives.WriteUInt32LittleEndian(buffer, (uint)ItemId);
        Length += 4;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[Length..], Count);
        Length += 4;

        BinaryPrimitives.WriteUInt64LittleEndian(buffer[Length..], PricePerUnit);
        Length += 8;

        BinaryPrimitives.WriteSingleLittleEndian(buffer[Length..], TaxPercent);
        Length += 4;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[Length..], PerPlayerLimitCount);
        Length += 4;

        return Length;

    }
    public void Deserialize(ReadOnlySpan<byte> buffer)
    {
        
        Length = 0;
        ItemId = (ItemType)BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        Length += 4;
        Count = BinaryPrimitives.ReadUInt32LittleEndian(buffer[Length..]);
        Length += 4;

        PricePerUnit = BinaryPrimitives.ReadUInt64LittleEndian(buffer[Length..]);
        Length += 8;
        TaxPercent = BinaryPrimitives.ReadSingleLittleEndian(buffer[Length..]);
        Length += 4;
        PerPlayerLimitCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer[Length..]);
        Length += 4;

    }

}