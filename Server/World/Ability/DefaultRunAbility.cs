using System.Numerics;
using Server.World.Effects;
using Server.World.Effects.AbilityStatusEffects;
using Server.World.Zone;
using Server.World.Zone.Entities;
using Shared.Ability;
using Shared.MasteryTree.Rewards;
using Shared.StatusEffect;

namespace Server.World.Ability;

public class DefaultRunAbility : AbilityBase
{
    
    public DefaultRunAbility(AbilityTypes abilityId) : base (abilityId)
    {
        

    }

    public override void OnApply(LivingEntity caster, Vector3? targetPos, LivingEntity? targetEntity, WorldZone region)
    {
        
        var effect = new StatModifierEffect(StatType.MoveSpeed, 12, DllData.Duration, StatusEffect.StatModifier);
        caster.ApplyStatusEffect(effect, caster);
        CurrentCooldown = DllData.Cooldown;

    }
    
    

}