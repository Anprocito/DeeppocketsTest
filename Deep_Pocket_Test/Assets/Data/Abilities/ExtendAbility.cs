using System.Collections.Generic;
using UnityEngine;

namespace DeepPocket.Data
{
    // Igual que Burst pero con el alcance extendido hacia afuera (y desde el cuerpo): una embestida.
    [CreateAssetMenu(menuName = "DeepPocket/Abilities/Extend", fileName = "ExtendAbility")]
    public class ExtendAbility : SpecialAbilityDefinition
    {
        [Min(0f)] public float damageMultiplier = 1.3f;
        [Min(0f)] public float extraRange = 3.5f;
        [Min(1)] public int maxTargets = 4;

        static readonly List<IDamageable> targets = new List<IDamageable>();

        public override Formation GetArea(AbilityContext ctx)
        {
            Formation f = ctx.formation;
            return new Formation(f.angleStartDeg, f.angleEndDeg, 0f, f.outerRadius + extraRange);
        }

        public override void Execute(AbilityContext ctx)
        {
            TargetQuery.Gather(ctx.origin, GetArea(ctx), ctx.enemyMask, maxTargets, targets, 0.3f, ctx.yawOffsetDeg);
            float damage = ctx.baseDamage * damageMultiplier;
            for (int i = 0; i < targets.Count; i++) targets[i].TakeDamage(damage, ctx.source);
        }
    }
}
