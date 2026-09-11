using System.Buffers.Binary;
using System.Text;
using Shared.Udp.Interfaces;

namespace Shared.Udp.Packets.Category.Settlement;

public struct S2C_TreasureHistoryUpdatePacket : INetworkPacket
{
    
    public int Length {get; private set;}
    public long Timestamp;
    public ushort NameLength;
    public string Name;
    public ulong Amount;
    public C2S_TreasuryActionPacket.TreasuryActionType ActionType;

    public int Serialize(Span<byte> buffer)
    {
        Length = 0;

        BinaryPrimitives.WriteInt64LittleEndian(buffer, Timestamp);
        Length += 8;

        ushort nameLength = (ushort)Encoding.UTF8.GetByteCount(Name ?? string.Empty);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer[Length..], nameLength);
        Length += 2;

        if(nameLength > 0)
        {
            Encoding.UTF8.GetBytes(Name, buffer[Length..]);
        }

        Length += nameLength;
        BinaryPrimitives.WriteUInt64LittleEndian(buffer[Length..], Amount);
        Length += 8;

        buffer[Length] = (byte)ActionType;
        Length++;

        return Length;

    }

    public void Deserialize(ReadOnlySpan<byte> buffer)
    {
        
        Length = 0;
        Timestamp = BinaryPrimitives.ReadInt64LittleEndian(buffer);
        Length += 8;
        NameLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[Length..]);
        Length += 2;
        Name = Encoding.UTF8.GetString(buffer.Slice(Length, NameLength));
        Length += NameLength;
        Amount = BinaryPrimitives.ReadUInt64LittleEndian(buffer[Length..]);
        Length += 8;
        ActionType = (C2S_TreasuryActionPacket.TreasuryActionType)buffer[Length];
        Length ++;


    }

}