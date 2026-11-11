using System.Collections.Generic;
using UnityEngine;

namespace DeepPocket.Data
{
    // El golpe sísmico del martillo: se dispara siempre, haya o no enemigos, para que el jugador
    // pueda anticiparlo. Daña todo en un radio alrededor del jugador, sin importar la zona del arma.
    [CreateAssetMenu(menuName = "DeepPocket/Abilities/Ground Slam", fileName = "GroundSlamAbility")]
    public class GroundSlamAbility : SpecialAbilityDefinition
    {
        [Min(0.5f)] public float radius = 5f;
        [Min(0f)] public float damageMultiplier = 1.6f;
        [Tooltip("Opcional: efecto visual que se instancia en la posición del jugador.")]
        public GameObject vfxPrefab;

        static readonly List<IDamageable> targets = new List<IDamageable>();

        public override Formation GetArea(AbilityContext ctx) => Formation.FullCircle(0f, radius);

        public override void Execute(AbilityContext ctx)
        {
            TargetQuery.GatherRadius(ctx.origin, radius, ctx.enemyMask, targets);
            float damage = ctx.baseDamage * damageMultiplier;
            for (int i = 0; i < targets.Count; i++) targets[i].TakeDamage(damage, ctx.source);

            if (vfxPrefab != null)
            {
                GameObject vfx = Object.Instantiate(vfxPrefab, ctx.origin, Quaternion.identity);
                vfx.transform.localScale = Vector3.one * (radius * 2f);
                Object.Destroy(vfx, 1f);
            }
        }
    }
}
