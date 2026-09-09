
using System.Buffers.Binary;
using Shared.Udp.Interfaces;

namespace Shared.Udp.Packets.Category.Settlement;

public struct C2S_TreasuryActionPacket : INetworkPacket
{
    public enum TreasuryActionType : byte
    {
        Withdraw = 0,
        Deposit = 1
    }
    
    public int Length {get; private set;}
    public TreasuryActionType ActionType; 
    public ulong Amount;

    public int Serialize(Span<byte> buffer)
    {
        
        Length = 0;
        buffer[Length] = (byte)ActionType;
        Length ++;
        BinaryPrimitives.WriteUInt64LittleEndian(buffer[Length..], Amount);
        Length += 8;

        return Length;

    }

    public void Deserialize(ReadOnlySpan<byte> buffer)
    {
        
        Length = 0;
        ActionType = (TreasuryActionType)buffer[Length];
        Length++;
        Amount = BinaryPrimitives.ReadUInt64LittleEndian(buffer[Length..]);
        Length += 8;

    }


}