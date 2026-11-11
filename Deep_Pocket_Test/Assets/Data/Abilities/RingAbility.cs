using System.Collections.Generic;
using UnityEngine;

namespace DeepPocket.Data
{
    // Golpea los 360° alrededor del jugador, pero solo en la banda de distancia del arma.
    [CreateAssetMenu(menuName = "DeepPocket/Abilities/Ring", fileName = "RingAbility")]
    public class RingAbility : SpecialAbilityDefinition
    {
        [Min(0f)] public float damageMultiplier = 1.2f;
        [Min(1)] public int maxTargets = 8;

        static readonly List<IDamageable> targets = new List<IDamageable>();

        public override Formation GetArea(AbilityContext ctx)
            => Formation.FullCircle(ctx.formation.innerRadius, ctx.formation.outerRadius);

        public override void Execute(AbilityContext ctx)
        {
            TargetQuery.Gather(ctx.origin, GetArea(ctx), ctx.enemyMask, maxTargets, targets, 0.3f, 0f);
            float damage = ctx.baseDamage * damageMultiplier;
            for (int i = 0; i < targets.Count; i++) targets[i].TakeDamage(damage, ctx.source);
        }
    }
}
