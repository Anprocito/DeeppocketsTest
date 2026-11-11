using System.Collections.Generic;
using UnityEngine;

namespace DeepPocket.Data
{
    // Golpea a todos los enemigos dentro de la propia zona del arma con daño extra.
    [CreateAssetMenu(menuName = "DeepPocket/Abilities/Burst", fileName = "BurstAbility")]
    public class BurstAbility : SpecialAbilityDefinition
    {
        [Min(0f)] public float damageMultiplier = 1.5f;
        [Min(1)] public int maxTargets = 8;

        static readonly List<IDamageable> targets = new List<IDamageable>();

        public override void Execute(AbilityContext ctx)
        {
            TargetQuery.Gather(ctx.origin, ctx.formation, ctx.enemyMask, maxTargets, targets, 0.3f, ctx.yawOffsetDeg);
            float damage = ctx.baseDamage * damageMultiplier;
            for (int i = 0; i < targets.Count; i++) targets[i].TakeDamage(damage, ctx.source);
        }
    }
}
