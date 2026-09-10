using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading;
using Godot;
using Shared.Udp.Interfaces;
using Shared.Udp.Packets;
using Shared.Udp.Packets.Category;
using Shared.Udp.Packets.Category.Game;
using Shared.Udp.Packets.Category.Game.Ability;
using Shared.Udp.Packets.Category.Game.Projectile;
using Shared.Udp.Packets.Category.MasteryTree;
using Shared.Udp.Packets.Category.Settlement;


namespace Client.NO_NODE;

public class PacketReaderClient
{
    
    private byte[] _buffer = new byte[1024];
    private ConcurrentQueue<INetworkPacket> _networkPackets;
    private readonly Socket _socket;
    private Thread _readerThread;

    private bool isRunning = false;

    public PacketReaderClient(Socket socket, ConcurrentQueue<INetworkPacket> packetQueue)
    {

        _socket = socket;
        _networkPackets = packetQueue;

    }

    public void Start()
    {

        _readerThread = new Thread(Gateway);
        _readerThread.Priority = ThreadPriority.AboveNormal;
        isRunning = true;
        _readerThread.Start();

    }
    public void Stop()
    {
        
        if (!isRunning) return;

        isRunning = false;

        try
        {
            
            _socket?.Close();

        }
        catch {}

        _readerThread?.Join(1000);

    }

    private void Gateway()
    {
        
        while(isRunning)
        {


            try
            {
                
                int length = _socket.Receive(_buffer);
                if (length < 2) continue;
                
                PacketTypes packetType = (PacketTypes)BinaryPrimitives.ReadUInt16LittleEndian(_buffer[..2]);

                ReadOnlySpan<byte> payload = _buffer[2..];

                switch (packetType)
                {
                    
                    case PacketTypes.S2C_HandshakeSuccess:
                        {
                            
                            var packet = PacketSerialier.Deserialize<S2C_HandshakeSuccessPacket>(payload);
                            _networkPackets.Enqueue(packet);

                        }
                    break;

                    case PacketTypes.S2C_SpawnEntity:
                        {
                            var packet = PacketSerialier.Deserialize<S2C_SpawnEntityPacket>(payload);
                            _networkPackets.Enqueue(packet);
                        }
                    break;

                    case PacketTypes.S2C_MoveEntity:
                        {
                            
                            var packet = PacketSerialier.Deserialize<S2C_MoveEntityPacket>(payload);
                            _networkPackets.Enqueue(packet);

                        }
                    break;

                    case PacketTypes.S2C_ItemDiff:
                        {
                            var packet = PacketSerialier.Deserialize<S2C_ItemDiffPacket>(payload);
                            _networkPackets.Enqueue(packet);
                        }
                    break;

                    case PacketTypes.S2C_RemoveEntity:
                        {
                            
                            var packet = PacketSerialier.Deserialize<S2C_RemoveEntityPacket>(payload);
                            _networkPackets.Enqueue(packet);

                        }
                    break;
                    case PacketTypes.S2C_ChangeRegion:
                        {
                            
                            var packet = PacketSerialier.Deserialize<S2C_ChangeRegionPacket>(payload);
                            _networkPackets.Enqueue(packet);

                        }
                        break;

                    case PacketTypes.S2C_EntityDamageTaked:
                        {
                            var packet = PacketSerialier.Deserialize<S2C_EntityDamageTakedPacket>(payload);
                            _networkPackets.Enqueue(packet);
                        }
                    break;

                    case PacketTypes.S2C_PlayerExpSync:
                        {
                            
                            var packet = PacketSerialier.Deserialize<S2C_PlayerExpSyncPacket>(payload);
                            _networkPackets.Enqueue(packet);

                        }
                    break;

                    case PacketTypes.S2C_BranchUpdate:
                        {
                            var packet = PacketSerialier.Deserialize<S2C_BranchUpdatePacket>(payload);
                            _networkPackets.Enqueue(packet);
                        }
                    break;

                    case PacketTypes.S2C_StatsSync:
                        {
                            var packet = PacketSerialier.Deserialize<S2C_StatsSyncPacket>(payload);
                            _networkPackets.Enqueue(packet);
                        }
                    break;

                    case PacketTypes.S2C_PlayerAbilitySync:
                        {
                            var packet = PacketSerialier.Deserialize<S2C_PlayerAbilitySyncPacket>(payload);
                            _networkPackets.Enqueue(packet);
                        }
                    break;

                    case PacketTypes.S2C_CastAbilityFailed:
                        {
                            var packet = PacketSerialier.Deserialize<S2C_CastAbilityFailedPacket>(payload);
                            _networkPackets.Enqueue(packet);
                        }
                    break;
                    case PacketTypes.S2C_AbilityCasted:
                        {
                            var packet = PacketSerialier.Deserialize<S2C_AbilityCastedPacket>(payload);
                            _networkPackets.Enqueue(packet);
                        }
                    break;
                    case PacketTypes.S2C_CastAbilitySuccessful:
                        {
                            var packet = PacketSerialier.Deserialize<S2C_CastAbilitySuccessfulPacket>(payload);
                            _networkPackets.Enqueue(packet);
                        }
                    break;
                    case PacketTypes.S2C_EntityMoveSpeedChanged:
                        {
                             var packet = PacketSerialier.Deserialize<S2C_EntityMoveSpeedChangedPacket>(payload);
                            _networkPackets.Enqueue(packet);
                        }
                    break;
                    case PacketTypes.C2S_AdminConsoleCommand:
                        {
                            var packet = PacketSerialier.Deserialize<C2S_AdminConsoleCommandPacket>(payload);
                            _networkPackets.Enqueue(packet);
                        }
                    break;

                    case PacketTypes.S2C_ProjectileCreated:
                        {
                            var packet = PacketSerialier.Deserialize<S2C_ProjectileCreatedPacket>(payload);
                            _networkPackets.Enqueue(packet);
                        }
                    break;
                    case PacketTypes.S2C_ProjectileDeleted:
                        {
                            var packet = PacketSerialier.Deserialize<S2C_ProjectileDeletedPacket>(payload);
                            _networkPackets.Enqueue(packet);
                        }
                    break;
                    case PacketTypes.S2C_SilverChanged:
                        {
                            var packet = PacketSerialier.Deserialize<S2C_SilverChangedPacket>(payload);
                            _networkPackets.Enqueue(packet);
                        }
                    break;

                    case PacketTypes.S2C_TreasureUpdate:
                        {
                            var packet = PacketSerialier.Deserialize<S2C_TreasureUpdatePacket>(payload);
                            _networkPackets.Enqueue(packet);
                        }
                    break;

                    default:
                        {
                            GD.PrintErr($"[PACK READER] Unknown packet type.");
                        }
                        break;
                    
                }

            }
            catch (Exception e)
            {

                GD.PrintErr($"[PACKET READER] Error:{e.Message}");

            }
           

        }


    }

}