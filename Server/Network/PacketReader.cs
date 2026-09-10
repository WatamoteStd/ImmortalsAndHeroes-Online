
using Shared.Udp.Packets;
using System.Buffers.Binary;
using Shared.Udp.Packets.Category;
using System.Net;
using Shared.Udp.Packets.Category.Game;
using Server.Pools.Session;
using Server.Network.Interfaces;
using System.Buffers;

namespace Server.Network;

public class PacketReader
{
    
    private readonly Lazy<ISessionPacketHandler> _sessionManagerApi;
    private IWorldHolder? _worldApi;

    public PacketReader(Lazy<ISessionPacketHandler> manager)
    {
        _sessionManagerApi = manager;
    }
    public void InitializeWorld(IWorldHolder world)
    {
        _worldApi = world;
    }

    public void PacketDeserialize(byte[] rawData, ushort length, UserSession session)
    {
        
        ReadOnlySpan<byte> clearSpan = rawData.AsSpan(0, length);

        PacketTypes packetType = (PacketTypes)BinaryPrimitives.ReadUInt16LittleEndian(clearSpan);

        switch (packetType)
        {
            
            case PacketTypes.C2S_Handshake:
                {
                    
                    var packet = PacketSerialier.Deserialize<C2S_HandshakePacket>(clearSpan[2..]);

                    _ = _sessionManagerApi.Value.HandshakeRequest(packet.Ticket, session.IpEnd);
                    ArrayPool<byte>.Shared.Return(rawData);

                }
            break;

            case PacketTypes.C2S_MoveRequest:
            case PacketTypes.C2S_ChangeRegionRequest:
            case PacketTypes.C2S_AttackRequest:
            case PacketTypes.C2S_MasteryTreeLearnRequest:
            case PacketTypes.C2S_CastAbilityRequest:
            case PacketTypes.C2S_AdminConsoleCommand:
            case PacketTypes.C2S_TreasuryAction:
            case PacketTypes.C2S_RoyalContractCreateRequest:
                _worldApi?.EnqueueCommand(new NetworkCommand
                {
                    Session = session,
                    Data = rawData,
                    PacketType = packetType,
                    Length = length
                });
            break;

            default:
                {
                    ArrayPool<byte>.Shared.Return(rawData);
                    Console.WriteLine("[Packet Reader] Unknown packet received");
                }
                break;

        }

    }

    public PacketTypes ReadPacketType(ReadOnlySpan<byte> data)
    {
        
        PacketTypes packetType = (PacketTypes)BinaryPrimitives.ReadUInt16LittleEndian(data);

        return packetType;
    }

}