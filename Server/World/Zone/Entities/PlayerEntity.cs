using Shared.Characters;
using System.Numerics;
using Server.World.Zone.Entities;
using Server.World.Inventory;
using Shared.Items;
using Server.World.Zone.Intefaces;
using Server.World.MasteryTree;
using Shared.MasteryTree;
using Shared.MasteryTree.Rewards;
using Shared.Udp.Packets.Category.Game;
using Server.World.Ability;
using Shared.Ability;
using Server.World.Zone.Entities.Ability;
using Server.World.Zone.Projectiles;
using Shared.ProjectilesData;

namespace Server.World;

public class PlayerEntity : LivingEntity
{

    public event Action<ushort, ItemType, ushort>? OnInventoryChanged;
    public event Action<int, int>? OnExpChanged; // this newExp totalExp
    public event Action<int, int>? OnSilverChanged;
    public event Action<MasteryNodeId, uint, ushort>? OnBranchUpdate;
    public event Action? OnAbilityUpdate;

    public enum State { Idle, Move, Chase, Attack, Cast, ProtectedCast, Controlled, Respawning}
    public State CurrentState = State.Idle;
    
    public uint PlayerId {get; private set;}
    public int Silver {get; private set;}
    public int Exp {get; private set;}
    protected LivingEntity _currentEnemy = null!;

    public InventoryBase Inventory = new InventoryBase(10);
    public PlayerMasteryTree MasteryTree {get;}

    // ABILITY

    private DefaultRunAbility _runAbility = new DefaultRunAbility(AbilityTypes.DefaulthRun);

    public PlayerEntity(uint entityId, Vector3 pos, EntityType type, string name, uint playerId, uint regionId, int silver) : base(entityId, pos, type, regionId)
    {
        Name = name;
        PlayerId = playerId;
        Silver = silver;

        Inventory.OnItemChanged += (slotIndex, item, count) =>
        {
            OnInventoryChanged?.Invoke(slotIndex, item,count);
        };

        MasteryTree = new PlayerMasteryTree(this);

        MasteryTree!.OnBranchUpdate += (branchId, exp, lvl) =>
        {
            OnBranchUpdate?.Invoke(branchId, exp, lvl);
        };

        _abilityComponent.OnAbilityUpdate += () => { OnAbilityUpdate?.Invoke();};

    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        if (!IsAlive) return;

        switch (CurrentState)
        {
            
            case State.Idle:

            break;

            case State.Move:
                {
                    
                    Move(deltaTime);

                }
            break;

            case State.Chase:
                {
                    
                    if (!_currentEnemy.IsValidEntity())
                    {
                        _currentEnemy = null!;
                        CurrentState = State.Idle;
                        _moveTarget = Position;
                        return;

                    }
                    if (!IsInAttackRadius(_currentEnemy))
                    {
                        
                        _moveTarget = _currentEnemy.Position;
                        Move(deltaTime);
                        return;

                    }
                    CurrentState = State.Attack;

                }
            break;

            case State.Attack:
                {
                    
                    if (!_currentEnemy.IsValidEntity())
                    {
                        
                        _currentEnemy = null!;
                        CurrentState = State.Idle;
                        return;

                    }

                    if (!IsInAttackRadius(_currentEnemy))
                    {
                        CurrentState= State.Chase;
                        return;
                    }

                    if (_currentAttackCooldown > 0) return;

                    if (_currentEnemy is IDamageable damageable)
                    {

                        if (TypeAttack == AttackType.Melee)
                        {
                            damageable.TakeDamage(DamageTypes.Physical, (int)BaseDamage, this);
                        }

                        else
                        {

                            ProjectileRegistry.TryGetProjectile(ProjectileType, out var prjData);
                            
                            Projectile prj = new Projectile
                            {
                                
                                Position = Position,
                                Speed = prjData.BaseSpeed,
                                Target = _currentEnemy,
                                Caster = this,
                                Damage = BaseDamage,
                                DamageType = DamageTypes.Physical,
                                Type = ProjectileType,
                                Radius = prjData.Radius,
                                Height = prjData.Height

                            };
                            
                            RaiseRangeAttackCommited(prj);

                        }
                        _currentAttackCooldown = _attackCooldown;

                    }


                }
            break;

        }
    }

    public void MoveToPosition(Vector3 pos)
    {
        
        _moveTarget = pos;
        CurrentState = State.Move;

    }

    public virtual void SetAttackTarget(LivingEntity entity)
    {
    
        _currentEnemy = entity;
        CurrentState = State.Chase;

    }

    public void AddExp(int exp)
    {
        Exp += exp;
        OnExpChanged?.Invoke(exp, Exp);
    }
    public void ChangeSilver(int silver)
    {
        Silver += silver;
        OnSilverChanged?.Invoke(silver, Silver);
    }

    public void AddBranchExp(MasteryNodeId id)
    {
        
        MasteryTree.AddExp(id);

    }


    #region INVENTORY

    public ushort AddItem(ItemType item, ushort count)
    {
        ushort less = Inventory.AddItem(item, count);

        return less;
    }

    public bool RemoveItem(ItemType item, ushort count)
    {
        
        bool isOk = Inventory.RemoveItem(item, count);
        return isOk;


    }

    #endregion



    public override void ClearAllSubscriptions()
    {
        base.ClearAllSubscriptions();
        OnInventoryChanged = null!; 
        OnExpChanged = null!;
        OnBranchUpdate = null!;
        OnAbilityUpdate = null;
    }
    

}