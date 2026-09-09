using System;
using System.Numerics;
using Server.Network.Interfaces;
using Server.World;
using Server.World.Zone.Entities;
using Shared.DataTransferObjects;
using Shared.Udp.Packets.Category;
using Shared.Udp.Packets;
using Shared.Udp.Packets.Category.Game;
using System.Collections.Concurrent;
using Server.Pools.Session;
using System.Buffers.Binary;
using System.Buffers;
using Shared.Udp.Packets.Category.MasteryTree;
using Shared.MasteryTree;
using Shared.Udp.Packets.Category.Game.Ability;
using Shared.Zone;
using Server.World.Zone.Settlement;
using Shared.Udp.Packets.Category.Settlement;

namespace Server.World.Zone;

public class WorldHolder : IWorldHolder
{

    public enum ZoneType { World, City, Capital, Dungeon}
    public Dictionary<uint, PlayerEntity> idToPlayer = new Dictionary<uint, PlayerEntity>(); // USERID
    public Dictionary<uint, WorldZone> idToZone = new Dictionary<uint, WorldZone>();
    public Dictionary<uint, BaseSettlement> idToSettlement = new Dictionary<uint, BaseSettlement>();


    private ConcurrentQueue<NetworkCommand> CommandsQueue = new ConcurrentQueue<NetworkCommand>();
    private ConcurrentQueue<HandshakeResponseDto> InitPlayerQueue = new();

    
    private readonly IWorldBroadcaster _broadcaster;
    public IWorldBroadcaster Broadcaster => _broadcaster;

    private AdminConsoleController _consoleController;

    public WorldHolder(IWorldBroadcaster broadcaster)
    {
        _broadcaster = broadcaster;
        _consoleController = new AdminConsoleController(Broadcaster);


        // SETTLEMENTS ==============================================
        CapitalSettlement chershia = new CapitalSettlement(1, "Chershia");
        CitySettlement hiacher = new CitySettlement(2, "Hiacher", chershia);
        VillageSettlement cesi = new VillageSettlement(3, "Cesi", hiacher);

        idToSettlement[chershia.Id] = chershia;
        idToSettlement[hiacher.Id] = hiacher;
        idToSettlement[cesi.Id] = cesi;




        // REGIONS ====================================================

        WorldZone startZone = new WorldZone(this, ZoneType.World, 0);
        startZone.Rules = ZoneRules.AllowPvE;

        WorldZone cityZone = new WorldZone(this, ZoneType.City, 1)
        {
            Rules = ZoneRules.None,
            Settlement = hiacher
        };


        WorldZone semiZone = new WorldZone(this, ZoneType.World, 2);
        semiZone.Rules = ZoneRules.AllowPvP | ZoneRules.AllowPvE | ZoneRules.FullLoot;

        WorldZone capital = new WorldZone(this, ZoneType.Capital, 3)
        {
            Rules = ZoneRules.None,
            Settlement = chershia
        };

        idToZone[0] = startZone;
        idToZone[1] = cityZone;
        idToZone[2] = semiZone;
        idToZone[3] = capital;
    }

    public void Update(float deltaTime)
    {

        while(InitPlayerQueue.TryDequeue(out var newData))
        {
            AddPlayer((uint)newData.RegionId, newData);
        }

        while(CommandsQueue.TryDequeue(out var cmd))
        {

            switch (cmd.PacketType)
            {
                
                case PacketTypes.C2S_MoveRequest:
                    {
                        var packet = PacketSerialier.Deserialize<C2S_MoveRequestPacket>(cmd.Data[2..]);
                        MovePlayer(cmd.Session.UserId, packet);

                        ArrayPool<byte>.Shared.Return(cmd.Data);
                    }
                break;
                case PacketTypes.S2C_RemoveEntity: // INTERNAL PACKET UNIQUE LOGIC!!!
                    {
                        if (idToPlayer.TryGetValue(cmd.Session.UserId, out PlayerEntity? player))
                        {
                            RemovePlayer(player.RegionId, player.PlayerId);
                            SuccessfulDeletePlayer(cmd.Session);
                            
                        }
                        else
                        {
                            Console.WriteLine($"[WORLD HOLDER] Can't delete Userd:{cmd.Session.UserId}");
                        }                       
                    }
                break;

                case PacketTypes.C2S_ChangeRegionRequest:
                    {
                        
                        var packet = PacketSerialier.Deserialize<C2S_ChangeRegionRequestPacket>(cmd.Data[2..]);
                        ChangePlayerRegion((uint)cmd.Session.UserId, packet);

                        ArrayPool<byte>.Shared.Return(cmd.Data);

                    }
                break;

                case PacketTypes.C2S_AttackRequest:
                    {
                        
                        var packet = PacketSerialier.Deserialize<C2S_AttackRequestPacket>(cmd.Data[2..]);
                    
                        PlayerAttackRequest((uint)cmd.Session.UserId, packet.Id);

                        ArrayPool<byte>.Shared.Return(cmd.Data);

                    }
                break;

                case PacketTypes.C2S_MasteryTreeLearnRequest:
                    {
                        
                        var packet = PacketSerialier.Deserialize<C2S_MasteryTreeLearnRequestPacket>(cmd.Data[2..]);
                        PlayerBranchLearnRequest(cmd.Session.UserId, packet.BranchId);

                        ArrayPool<byte>.Shared.Return(cmd.Data);

                    }
                break;

                case PacketTypes.C2S_CastAbilityRequest:
                    {
                        
                        var packet = PacketSerialier.Deserialize<C2S_CastAbilityRequestPacket>(cmd.Data[2..]);
                        Console.WriteLine($"[WorldHolder] Skill Packet! slot:{packet.Slot} | Pos: x:{packet.PosX} y:{packet.PosY} z:{packet.PosZ} entityId:{packet.TargetEntityId} ");
                        PlayerSkillActivationRequest(cmd.Session.UserId, packet);

                        ArrayPool<byte>.Shared.Return(cmd.Data);

                    }
                break;

                case PacketTypes.C2S_AdminConsoleCommand:
                    {
                        
                        var packet = PacketSerialier.Deserialize<C2S_AdminConsoleCommandPacket>(cmd.Data[2..]);
                        ArrayPool<byte>.Shared.Return(cmd.Data);

                        if (idToPlayer.TryGetValue(cmd.Session.UserId, out var player) && idToZone.TryGetValue(player.RegionId, out var zone))
                        {
                            _consoleController.ExecuteCommand(packet, player, zone);
                        }
                        else
                        {
                            Console.WriteLine($"[WorldHolder(admin)] Someone who never exists try to execute command");
                        }

                    }
                break;

                //SETTLEMENT PACKETS
                case PacketTypes.C2S_TreasuryAction:
                    {
                        
                        var packet = PacketSerialier.Deserialize<C2S_TreasuryActionPacket>(cmd.Data[2..]);
                        if (idToPlayer.TryGetValue(cmd.Session.UserId, out var player) && idToZone.TryGetValue(player.RegionId, out var zone))
                        {
                            
                            if (zone.Settlement != null)
                            {
                                if (packet.ActionType == C2S_TreasuryActionPacket.TreasuryActionType.Withdraw)
                                {
                                    
                                    bool isSuc = zone.Settlement.TryRemoveSilver(packet.Amount);
                                    if (isSuc) player.ChangeSilver((int)packet.Amount);
                                    Console.WriteLine($"[WorldHolder] Withdraw action by player:{player.Name} is:{isSuc} Current silver of {zone.Settlement.Name}:{zone.Settlement.Silver}");

                                }
                                if (packet.ActionType == C2S_TreasuryActionPacket.TreasuryActionType.Deposit)
                                {
                                    if (player.Silver >= (int)packet.Amount)
                                    {
                                        
                                        zone.Settlement.AddSilver(packet.Amount);
                                        player.ChangeSilver(-(int)packet.Amount);
                                        Console.WriteLine($"[WorldHolder] Withdraw action by player:{player.Name} success! Current silver of {zone.Settlement.Name}:{zone.Settlement.Silver}");

                                    }
                                    else
                                    {
                                        Console.WriteLine($"[WorldHolder] Withdraw action by player:{player.Name} blocked! Not enought silver");
                                    }
                        
                                }
                            }

                        }
                        ArrayPool<byte>.Shared.Return(cmd.Data);

                    }
                break;

            }

        }
        
        foreach (var zone in idToZone.Values)
        {
            zone.Update(deltaTime);
        }

    }

    public void AddPlayer(uint zoneId, HandshakeResponseDto character)
    {
        
        if (idToZone.TryGetValue(zoneId, out WorldZone? zone))
        {

            Vector3 startPos = new Vector3(character.PosX, character.PosY, character.PosZ);
            PlayerEntity newPlayer = new PlayerEntity((uint)character.Id, startPos, character.Type, character.Name,(uint)character.UserId, (uint)character.RegionId, (int)character.Silver);
            
            zone.AddPlayer(newPlayer);
            idToPlayer[(uint)character.UserId] = newPlayer;
            Console.WriteLine($"[WORLD] Added new player to zoneId{zoneId}");
            return;

        }
        Console.WriteLine($"[WORLD] Can't add player to ZoneId:{zoneId}. Doest exists.");

    }

    public void RemovePlayer(uint zoneId, uint playerId)
    {
        
        if (idToZone.TryGetValue(zoneId, out WorldZone? zone))
        {
            zone.RemovePlayer(playerId, notifySelf: true);
            idToPlayer.Remove(playerId);
            Console.WriteLine($"[WORLD HOLDER] PlayerID{playerId} leave from the world.");
        }

    }


    // ================================ FROM REGION TO PLAYER ==============================
    public void SlotUpdatePlayer(uint userId, S2C_ItemDiffPacket packet)
    {
        _broadcaster.SendToPlayer(userId, PacketTypes.S2C_ItemDiff, packet);
    }

    // =============================== FROM PLAYER TO REGION PACKETS ==================

    public void MovePlayer(uint userId, C2S_MoveRequestPacket packet)
    {
        if (idToPlayer.TryGetValue(userId, out PlayerEntity? player))
        {

            idToZone[player.RegionId].MovePlayer(player, packet.X, 1, packet.Z);
        }
    }

    public void ChangePlayerRegion(uint userId, C2S_ChangeRegionRequestPacket packet)
    {
        Console.WriteLine($"[DEBUG] ChangeRegion called with userId: {userId}");
        
        if (idToPlayer.TryGetValue(userId, out PlayerEntity? player))
        {
            
            if (player.RegionId != packet.RegionId) // WARNING! NOW THERE IS NO LEGIT CHECK 
            {
                
                if(idToZone.TryGetValue(player.RegionId, out WorldZone? oldRegion) && idToZone.TryGetValue(packet.RegionId, out WorldZone? newRegion))
                {
                    
                    oldRegion.RemovePlayer(player.PlayerId, notifySelf: false);
                    player.RegionId = newRegion.Id;

                    var changeRegPacket = new S2C_ChangeRegionPacket
                    {
                        CharacterId = player.EntityId,
                        RegionId = packet.RegionId
                    };
                    player.SetPosition(0,1,0);
                    player.MoveToPosition(new Vector3(0,1,0));
                    var characterDataPacket = new S2C_HandshakeSuccessPacket
                    {
                        Id = player.EntityId,
                        RegionId = player.RegionId,
                        Name = player.Name,
                        PosX = player.Position.X,
                        PosY = player.Position.Y,
                        PosZ = player.Position.Z,
                        UserId = player.PlayerId,
                        Type = player.ModelType,
                        CurrentHp = player.Health,
                        CurrentMp = player.Mana,
                        Silver = (uint)player.Silver
                    };

                    _broadcaster.SendToPlayer<S2C_ChangeRegionPacket>(player.PlayerId, PacketTypes.S2C_ChangeRegion, changeRegPacket);
                    _broadcaster.SendToPlayer<S2C_HandshakeSuccessPacket>(player.PlayerId, PacketTypes.S2C_HandshakeSuccess, characterDataPacket);
                    player.UpdateStat(Shared.MasteryTree.Rewards.StatType.None, 0);
                    newRegion.AddPlayer(player);


                    return;
                }

            }
            Console.WriteLine($"[CHANGE REGION] Invalid region params!");
            return;

        }
        Console.WriteLine($"[CHANGE REGION] Unknown player try to change region");

    }

    public void InitiateNewPlayer(HandshakeResponseDto data)
    {
        InitPlayerQueue.Enqueue(data);
    }


    public void EnqueueCommand(NetworkCommand cmd)
    {
        CommandsQueue.Enqueue(cmd);
    }
    public void SM_RemovePlayer(UserSession session)
    {
        
        var cmd = new NetworkCommand
        {
            Session = session,
            Data = null!,
            PacketType = PacketTypes.S2C_RemoveEntity,
            Length = 0
        };
        CommandsQueue.Enqueue(cmd);

    }
    public void SuccessfulDeletePlayer(UserSession session)
    {
        _broadcaster.API_RemoveSession(session);
    }

    public void PlayerAttackRequest(uint userId, uint entityId)
    {
        
        if (idToPlayer.TryGetValue(userId, out PlayerEntity? player) && idToZone.TryGetValue(player.RegionId, out WorldZone? zone))
        { 
            zone.PlayerAttackRequest(player, entityId);
        }
        else
        {
            Console.WriteLine($"[WorldHolder] UserId:{userId} attack request declined. Invalid data.");
            return;
        }

    }
    public void PlayerBranchLearnRequest(uint userId, MasteryNodeId branch)
    {
        
        if (idToPlayer.TryGetValue(userId, out PlayerEntity? player) && idToZone.TryGetValue(player.RegionId, out WorldZone? zone))
        {
            
            zone.PlayerBranch_AddExp(player, branch);

        }

    }

    public void PlayerSkillActivationRequest(uint userId, C2S_CastAbilityRequestPacket packet)
    {
        
        if (idToPlayer.TryGetValue(userId, out var player) && idToZone.TryGetValue(player.RegionId, out var zone)) 
        {
            
            zone.PlayerCastSkillRequest(player, packet);

        }

    }

    

}