using Server.World.Zone.Entities;
using Shared.Udp.Packets.Category.Game;
using Shared.Udp.Packets;
using System.Numerics;
using Shared.Items;
using Shared.Udp.Packets.Category;
using Shared.Characters;
using Server.World.Zone.Entities.Mobs;
using Server.World.Zone.RegionController;
using Shared.Items.DropTable;
using Shared.MasteryTree;
using Shared.Udp.Packets.Category.MasteryTree;
using Shared.Udp.Packets.Category.Game.Ability;
using Server.World.Zone.Entities.Ability;
using Shared.Ability.CastErrors;
using Server.World.Zone.Projectiles;
using Shared.Udp.Packets.Category.Game.Projectile;
using Shared.Zone;
using Server.World.Zone.Settlement;

namespace Server.World.Zone;

public class WorldZone
{
    public WorldHolder.ZoneType Type {get; private set;}
    public uint Id {get; private set;}
    private readonly WorldHolder _worldHolder;
    public BaseSettlement? Settlement { get; set; }

    public ZoneRules Rules {get; set;}

    public Dictionary<uint, PlayerEntity> Players {get; private set;} = new();
    public Dictionary<uint, EntityBase> Entities {get; private set;}= new();

    private static uint _currentMobId = 2_000_000;

    public ProjectileManager _projectileManager {get; private set;} = new();

    private RegionSpawner _spawner;
    

    private float latensy;
    private int iterationCount;

    public WorldZone(WorldHolder world, WorldHolder.ZoneType type, uint id)
    {
        
        _worldHolder = world;
        Type = type;
        Id = id;

        if (Type != WorldHolder.ZoneType.World)
        {
             _spawner = new RegionSpawnBuilder(this)
            .SetDensity(DensityModes.Near)
            .SetCapacity(0)
            .GroupsAllowed(false)
            .Build();

        }
        else
        {

            if (Id == 2)
            {
                
                _spawner = new RegionSpawnBuilder(this)
                .SetDensity(DensityModes.Normal)
                .SetCapacity(60)
                .GroupsAllowed(false)
                .AddMonster(EntityType.WolfWeak, 40)
                .AddMonster(EntityType.ForestBear, 20)
                .RespawnTime(20f)
                .Build();

            }
            else
            {
                
                _spawner = new RegionSpawnBuilder(this)
                .SetDensity(DensityModes.Near)
                .SetCapacity(60)
                .GroupsAllowed(false)
                .AddMonster(EntityType.WolfWeak, 30)
                .AddMonster(EntityType.ForestBear, 15)
                .AddMonster(EntityType.UnknownMage, 15)
                .RespawnTime(15f)
                .Build();

            }
        }

        _projectileManager.OnProjectileAdded += SendProjectileToRegion;
        _projectileManager.OnProjectileRemoved += SendProjectileDeleted;

    }

    public void Update(float deltaTime)
    {
        
        // DEBUG ===============
        latensy += deltaTime;
        if (latensy >= 45.0f)
        {
            Console.WriteLine($"[45s Debug| N:{iterationCount}] RegionId:{Id}. Players: {Players.Count} Entities:{Entities.Count}");
            latensy = 0f;
            iterationCount++;
        }

        if (Type == WorldHolder.ZoneType.World)
        {
            _spawner.Update(deltaTime);
        }

        _projectileManager.Update(deltaTime);

        foreach (var entity in Entities.Values)
        {
            entity.Update(deltaTime);    
        }


    }

    public void AddPlayer(PlayerEntity player)
    {

        var newPlayerPacket = new S2C_SpawnEntityPacket
        {
            Id = player.EntityId,
            Health = player.Health,
            Name = player.Name,
            PosX = player.Position.X,
            PosY = player.Position.Y,
            PosZ = player.Position.Z,
            Type = player.ModelType
        };

        foreach (var oldPlayer in Players.Values)
        {
            _worldHolder.Broadcaster.SendToPlayer(oldPlayer.PlayerId, PacketTypes.S2C_SpawnEntity, newPlayerPacket);
        }

        Players[player.PlayerId] = player;
        Entities[player.EntityId] = player;

        foreach (var entity in Entities.Values)
        {
            if (entity.EntityId == player.EntityId) continue;
            
            var oldEntityPacket = new S2C_SpawnEntityPacket
            {
                Id = entity.EntityId,
                Health = entity.Health,
                Name = entity.Name,
                PosX = entity.Position.X,
                PosY  = entity.Position.Y,
                PosZ = entity.Position.Z,
                Type = entity.ModelType
            };

            _worldHolder.Broadcaster.SendToPlayer(player.PlayerId, PacketTypes.S2C_SpawnEntity, oldEntityPacket);

        }

        var abilitySync = new S2C_PlayerAbilitySyncPacket
        {
            Slot0 = player.GetAbilitySlot(0),
            Slot1 = player.GetAbilitySlot(1),
            Slot2 = player.GetAbilitySlot(2),
            Slot3 = player.GetAbilitySlot(3),
            Slot4 = player.GetAbilitySlot(4),
            Slot5 = player.GetAbilitySlot(5),
        };
        _worldHolder.Broadcaster.SendToPlayer<S2C_PlayerAbilitySyncPacket>(player.PlayerId, PacketTypes.S2C_PlayerAbilitySync, abilitySync);
        
         player.OnInventoryChanged += (slotIndex, item, count) =>
        {
            
            var diffPacket = new S2C_ItemDiffPacket
            {
                CharacterId = player.EntityId,
                SlotIndex = slotIndex,
                Item = item,
                Count = count
            };

            _worldHolder.SlotUpdatePlayer(player.PlayerId, diffPacket);

        };
        player.OnMoved += (character, pos) =>
        {
            
            var movePacket = new S2C_MoveEntityPacket
            {
                Id = character.EntityId,
                PosX = character.Position.X,
                PosY = character.Position.Y,
                PosZ = character.Position.Z
            };

            foreach (var p in Players.Values)
            {
                _worldHolder.Broadcaster.SendToPlayer<S2C_MoveEntityPacket>(p.PlayerId, PacketTypes.S2C_MoveEntity, movePacket);
            }

        };
        player.OnDamageTaked += (character, dmg, attacker) =>
        {
            
            var packet = new S2C_EntityDamageTakedPacket
            {
                Id = character.EntityId,
                Damage = dmg,
                AttackerId = attacker.EntityId,
                ActualHealth = (uint)character.Health
            };

            foreach (var p in Players.Values)
            {
                _worldHolder.Broadcaster.SendToPlayer(p.PlayerId, PacketTypes.S2C_EntityDamageTaked, packet);
            }

        };
        player.OnExpChanged += (expChange, totalExp) =>
        {
            
            var packet = new S2C_PlayerExpSyncPacket
            {
                ExpDelta = expChange,
                TotalExp = (uint)totalExp
            };
            _worldHolder.Broadcaster.SendToPlayer<S2C_PlayerExpSyncPacket>(player.PlayerId, PacketTypes.S2C_PlayerExpSync, packet);

        };
        player.OnSilverChanged += (value, total) =>
        {
            var packet = new S2C_SilverChangedPacket
            {
                SilverValue = value,
                TotalSilver = total
            };
            _worldHolder.Broadcaster.SendToPlayer<S2C_SilverChangedPacket>(player.PlayerId, PacketTypes.S2C_SilverChanged, packet);
        };
        player.OnBranchUpdate += (branchId, exp, lvl) =>
        {
            var packet = new S2C_BranchUpdatePacket
            {
                BranchId = branchId,
                CurrentExp = exp,
                CurrentLevel = lvl
            };
            _worldHolder.Broadcaster.SendToPlayer<S2C_BranchUpdatePacket>(player.PlayerId, PacketTypes.S2C_BranchUpdate, packet);
        };
        player.OnStatsUpdated += (packet) =>
        {
            _worldHolder.Broadcaster.SendToPlayer<S2C_StatsSyncPacket>(player.PlayerId, PacketTypes.S2C_StatsSync, packet);
        };
        player.OnAbilityUpdate += () =>
        {
            var abl = new S2C_PlayerAbilitySyncPacket
            {
                Slot0 = player.GetAbilitySlot(0),
                Slot1 = player.GetAbilitySlot(1),
                Slot2 = player.GetAbilitySlot(2),
                Slot3 = player.GetAbilitySlot(3),
                Slot4 = player.GetAbilitySlot(4),
                Slot5 = player.GetAbilitySlot(5),
            };
            _worldHolder.Broadcaster.SendToPlayer<S2C_PlayerAbilitySyncPacket>(player.PlayerId, PacketTypes.S2C_PlayerAbilitySync, abl);
            
        };
        player.OnMoveSpeedChanged += (packet) =>
        {
            foreach (var pair in Players)
            {
                var p = pair.Value;
                _worldHolder.Broadcaster.SendToPlayer<S2C_EntityMoveSpeedChangedPacket>(p.PlayerId, PacketTypes.S2C_EntityMoveSpeedChanged, packet);
                
            }
        };
        player.OnRangeAttackCommited += _projectileManager.AddProjectile;
        

    }

    public void CreateEntity(EntityType type, Vector3 spawnPosition)
    {
        
        var entityData = EntityRegistry.GetEntityData(type);

        MonsterEntity newEntity = new MonsterEntity(GenerateNextMobId(), spawnPosition, type, Id, this);
        Entities[newEntity.EntityId] = newEntity;

        var packet = new S2C_SpawnEntityPacket
        {
            Id = newEntity.EntityId,
            Health = (int)entityData.BaseHealth,
            Name = entityData.Name,
            PosX = spawnPosition.X,
            PosY = spawnPosition.Y,
            PosZ = spawnPosition.Z,
            Type = type
        };

        foreach (var pair in Players)
        {
            var p = pair.Value;
            _worldHolder.Broadcaster.SendToPlayer<S2C_SpawnEntityPacket>(p.PlayerId, PacketTypes.S2C_SpawnEntity, packet);
        }

        newEntity.OnDamageTaked += (entity, damage, attacker) =>
        {
            var packet = new S2C_EntityDamageTakedPacket
            {
                Id = entity.EntityId,
                Damage = damage,
                AttackerId = attacker.EntityId,
                ActualHealth = (uint)entity.Health
            };

            foreach (var pair in Players)
            {
                var p = pair.Value;
                _worldHolder.Broadcaster.SendToPlayer(p.PlayerId, PacketTypes.S2C_EntityDamageTaked, packet);
            }
        };
        newEntity.OnMoved += (entity, pos) =>
        {
            var movePacket = new S2C_MoveEntityPacket
            {
                Id = entity.EntityId,
                PosX = entity.Position.X,
                PosY = entity.Position.Y,
                PosZ = entity.Position.Z
            };

            foreach (var pair in Players)
            {
                var p = pair.Value;
                _worldHolder.Broadcaster.SendToPlayer<S2C_MoveEntityPacket>(p.PlayerId, PacketTypes.S2C_MoveEntity, movePacket);
            }
        };
        newEntity.OnDead += (entity, attacker) =>
        {
            
            var removePacket = new S2C_RemoveEntityPacket
            {
                Id = entity.EntityId
            };

            foreach(var pair in Players)
            {
                var p = pair.Value;
                _worldHolder.Broadcaster.SendToPlayer<S2C_RemoveEntityPacket>(p.PlayerId, PacketTypes.S2C_RemoveEntity, removePacket);
            }

            if (entity is MonsterEntity monster)

            _spawner.EntityDie(monster);


            // LOOT GENERATE

            if (attacker is PlayerEntity player)
            {
                Console.WriteLine($"[Region#{Id}] {entity.Name}:{entity.EntityId} die! Killer:{player.Name}");
                
                var drops = LootTableManager.GetEntityDropTable(entity.ModelType);
                var entityData = EntityRegistry.GetEntityData(type);

                if (drops.Length > 0)
                {
                    
                        for (int i = 0; i < drops.Length; i++)
                    {
                        var drop = drops[i];
                    
                        if (Random.Shared.NextSingle() <= drop.DropChance)
                        {
                
                            ushort count = (ushort)Random.Shared.Next(drop.MinCount, drop.MaxCount + 1);
                            _ = player.Inventory.AddItem(drop.Item, count);

                        }

                    }

                }

                // EXP 
                if (entityData.MinExpReward is uint minExp && entityData.MaxExpReward is uint maxExp)
                {
                    
                    int expGained = Random.Shared.Next((int)minExp, (int)maxExp + 1);
                    player.AddExp(expGained);

                }


            }

        };

        newEntity.OnMoveSpeedChanged += (packet) =>
        {
            
            foreach (var pair in Players)
            {
                var p = pair.Value;
                _worldHolder.Broadcaster.SendToPlayer<S2C_EntityMoveSpeedChangedPacket>(p.PlayerId, PacketTypes.S2C_EntityMoveSpeedChanged, packet);

            }

        };
        newEntity.OnRangeAttackCommited += _projectileManager.AddProjectile;

    }


    public static uint GenerateNextMobId()
    {
        return Interlocked.Increment(ref _currentMobId);
    }


    public PlayerEntity? TryFindNearestPlayer(Vector3 pos, float radius)
    {
        float radiusSq = radius * radius;

        PlayerEntity? nearestPlayer = null;
        float minDistSq = radiusSq;

        foreach (var p in Players.Values)
        {
            
            if (!p.IsAlive) continue;

            float curDistSq = Vector3.DistanceSquared(p.Position, pos);

            if (curDistSq < minDistSq )
            {
                
                minDistSq = curDistSq;
                nearestPlayer = p;

            }

        }
        return nearestPlayer;

    }

    public List<LivingEntity> FindEntityInRadius(Vector3 pos, float radius)
    {
        
        List<LivingEntity> enemies = new List<LivingEntity>();

        foreach (var pair in Entities)
        {
            
            var ent = pair.Value;

            if (!ent.IsValidEntity() || !ent.IsAlive) continue;

            if (ent is not LivingEntity livEnt) continue;

            float totalRad = radius + livEnt.Radius;
            float totalRadSq = totalRad * totalRad;

            if (Vector3.DistanceSquared(livEnt.Position, pos) <= totalRadSq)
            {
                
                enemies.Add(livEnt);

            }

        }

        return enemies;

    }
    
    #region Player -> World || Server -> World
    public void MovePlayer(PlayerEntity player, float x, float y, float z)
    {
        player.MoveToPosition(new Vector3(x,1,z));

        foreach (var curPlayer in Players.Values)
        {
            var movePacket = new S2C_MoveEntityPacket
            {
                Id = player.EntityId,
                PosX = player.Position.X,
                PosY = player.Position.Y,
                PosZ = player.Position.Z
            };
            _worldHolder.Broadcaster.SendToPlayer(curPlayer.PlayerId, PacketTypes.S2C_MoveEntity, movePacket);
        }
    }

    public void RemovePlayer(uint playerId, bool notifySelf = false)
    {
        
        if (Players.TryGetValue(playerId, out PlayerEntity? player))
        {

            player.ClearAllSubscriptions();
            Players.Remove(player.PlayerId);
            Entities.Remove(player.EntityId);
            
            var packet = new S2C_RemoveEntityPacket { Id = player.EntityId};
        
            foreach (var p in Players.Values)
            {
                _worldHolder.Broadcaster.SendToPlayer(p.PlayerId, PacketTypes.S2C_RemoveEntity, packet);
            }

            if (notifySelf)
            {
                _worldHolder.Broadcaster.SendToPlayer(player.PlayerId, PacketTypes.S2C_RemoveEntity, packet);
            }

            Console.WriteLine($"[REGION {Id}] Character:{player.Name} has been removed from region!");
            return;

            }

        Console.WriteLine($"[REGION {Id}] Can't find PlayerId:{playerId} at this region");
        
    }

    public void PlayerAttackRequest(PlayerEntity player, uint entityId)
    {
        
        if (Entities.TryGetValue(entityId, out EntityBase? entity))
        {

            if (entity is LivingEntity living)
            {

                if (living is PlayerEntity pl && !Rules.HasFlag(ZoneRules.AllowPvP))
                {
                    Console.WriteLine($"Player:{player.Name} try to attack:{pl.Name} in non PvpZone");
                    return;
                }
                else if (living is MonsterEntity mons && !Rules.HasFlag(ZoneRules.AllowPvE))
                {
                    return;
                }
                
                player.SetAttackTarget(living);

            }

        }
        else
        {
            Console.WriteLine($"[Region:{Id}] Attack request declined. Can't find some entity.");
        }

    }

    public void PlayerBranch_AddExp(PlayerEntity player, MasteryNodeId branch)
    {
        player.AddBranchExp(branch);
    }


    public void PlayerCastSkillRequest(PlayerEntity player, C2S_CastAbilityRequestPacket packet)
    {
        
        Vector3 pos = new Vector3(packet.PosX, packet.PosY, packet.PosZ);
        CastResult result;

        if (packet.TargetEntityId == 0)
        { 
            result = player.TryCastAbility(packet.Slot, this, pos, null);
        }
        else if(Entities.TryGetValue(packet.TargetEntityId, out var entity) && entity is LivingEntity liveEntity)
        {
            result = player.TryCastAbility(packet.Slot, this, new Vector3(packet.PosX, packet.PosY, packet.PosZ), liveEntity);
        }
        else 
        {
            
            var pck = new S2C_CastAbilityFailedPacket
            {
                ResponseCode = AbilityCastErrors.InvalidTarget
            };

            _worldHolder.Broadcaster.SendToPlayer<S2C_CastAbilityFailedPacket>(player.PlayerId, PacketTypes.S2C_CastAbilityFailed, pck);
            Console.WriteLine($"[Region:{Id}] Can't cast");
            return;

        };

        if (result.IsSucces)
        {
            
            var sPacket = new S2C_CastAbilitySuccessfulPacket
            {
                Slot = result.Slot,
                CurrentCooldown = result.FinalCooldown
            };
            _worldHolder.Broadcaster.SendToPlayer<S2C_CastAbilitySuccessfulPacket>(player.PlayerId, PacketTypes.S2C_CastAbilitySuccessful, sPacket);

            var gPacket = new S2C_AbilityCastedPacket
            {
                CasterEntityId = player.EntityId,
                AbilityId = result.AbilityId,
                PosX = result.CastPosition.X,
                PosY = result.CastPosition.Y,
                PosZ = result.CastPosition.Z,
                TargetEntityId = result.EnemyId
            };

            foreach (var pair in Players)
            {
                var p = pair.Value;
                _worldHolder.Broadcaster.SendToPlayer<S2C_AbilityCastedPacket>(p.PlayerId, PacketTypes.S2C_AbilityCasted, gPacket);
            }


        }
        else
        {
            var ePacket = new S2C_CastAbilityFailedPacket
            {
                ResponseCode = result.Error
            };
            _worldHolder.Broadcaster.SendToPlayer<S2C_CastAbilityFailedPacket>(player.PlayerId, PacketTypes.S2C_CastAbilityFailed, ePacket);
        }
        

    }

    #endregion

    #region REGION SPAWNER 

    public void RespawnMonster(MonsterEntity entity, Vector3 pos)
    {
        entity.Respawn(pos);

        var packet = new S2C_SpawnEntityPacket
        {
            Id = entity.EntityId,
            Health = entity.Health,
            Name = entity.Name,
            PosX = pos.X,
            PosY = pos.Y,
            PosZ = pos.Z,
            Type = entity.ModelType
        };

        foreach(var pair in Players)
        {
            var p = pair.Value;
            _worldHolder.Broadcaster.SendToPlayer<S2C_SpawnEntityPacket>(p.PlayerId, PacketTypes.S2C_SpawnEntity, packet);
        }

    }

    #endregion


    #region Projectile


    private void SendProjectileToRegion(Projectile prj)
    {
        
        var packet = new S2C_ProjectileCreatedPacket
        {
            Id = prj.Id,
            CasterId = prj.Caster.EntityId,
            TargetId = prj.Target.EntityId,
            Type = prj.Type,
            Speed = prj.Speed
        };

        foreach(var pair in Players)
        {
            var p = pair.Value;
            _worldHolder.Broadcaster.SendToPlayer<S2C_ProjectileCreatedPacket>(p.PlayerId, PacketTypes.S2C_ProjectileCreated, packet);
        }

    }
    private void SendProjectileDeleted(ushort id)
    {
        
        var packet = new S2C_ProjectileDeletedPacket
        {
            Id = id
        };
        foreach(var pair in Players)
        {
            var p = pair.Value;
            _worldHolder.Broadcaster.SendToPlayer<S2C_ProjectileDeletedPacket>(p.PlayerId, PacketTypes.S2C_ProjectileDeleted, packet);
        }

    }


    #endregion

}