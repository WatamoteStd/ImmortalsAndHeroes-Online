using System.Buffers.Binary;
using Shared.Items;
using Shared.Udp.Interfaces;

namespace Shared.Udp.Packets.Category.Settlement;

public struct C2S_RoyalContractCreateRequestPacket : INetworkPacket
{
    
    public int Length {get; private set;}
    public ItemType ItemId;
    public uint TotalCount;
    public ulong PricePerUnit;
    public bool TaxFree;
    public float TaxPercent;
    public bool PerPlayerLimit;
    public uint PerPlayerLimitCount;

    public int Serialize(Span<byte> buffer)
    {
        
        Length = 0;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer, (uint)ItemId);
        Length += 4;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[Length..],TotalCount);
        Length += 4;
        BinaryPrimitives.WriteUInt64LittleEndian(buffer[Length..],PricePerUnit);
        Length += 8;
        buffer[Length] = (byte)(TaxFree ? 1 : 0);
        Length++;
        BinaryPrimitives.WriteSingleLittleEndian(buffer[Length..], TaxPercent);
        Length += 4;
        buffer[Length] = (byte)(PerPlayerLimit ? 1 : 0);
        Length++;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[Length..], PerPlayerLimitCount);
        Length += 4;

        return Length;

    }
    public void Deserialize(ReadOnlySpan<byte> buffer)
    {
        int offset = 0;

        ItemId = (ItemType)BinaryPrimitives.ReadUInt32LittleEndian(buffer[offset..]);
        offset += 4;

        TotalCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer[offset..]);
        offset += 4;

        PricePerUnit = BinaryPrimitives.ReadUInt64LittleEndian(buffer[offset..]);
        offset += 8;

        TaxFree = buffer[offset] != 0;
        offset++;

        TaxPercent = BinaryPrimitives.ReadSingleLittleEndian(buffer[offset..]);
        offset += 4;

        PerPlayerLimit = buffer[offset] != 0;
        offset++;

        PerPlayerLimitCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer[offset..]);
        offset += 4;

        Length = offset;
}


}