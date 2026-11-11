using System.Collections.Generic;
using UnityEngine;

namespace DeepPocket.Data
{
    // Busca objetivos dentro de una Formation. Usa buffers estáticos para no generar basura.
    public static class TargetQuery
    {
        static readonly Collider[] buffer = new Collider[128];
        static readonly List<(float sqr, IDamageable target)> scratch = new List<(float, IDamageable)>();
        static readonly HashSet<IDamageable> seen = new HashSet<IDamageable>();

        // maxTargets <= 0 significa sin límite. Resultados ordenados del más cercano al más lejano.
        public static void Gather(Vector3 origin, Formation formation, LayerMask mask, int maxTargets,
                                  List<IDamageable> results, float slack = 0.3f, float yawOffsetDeg = 0f)
        {
            results.Clear();
            scratch.Clear();
            seen.Clear();

            int count = Physics.OverlapSphereNonAlloc(origin, formation.outerRadius + slack, buffer, mask,
                                                      QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                Collider col = buffer[i];
                IDamageable damageable = col.GetComponentInParent<IDamageable>();
                if (damageable == null || !seen.Add(damageable)) continue;

                Component comp = damageable as Component;
                Vector3 pos = comp != null ? comp.transform.position : col.transform.position;
                Vector3 offset = pos - origin;
                if (!formation.Contains(offset, slack, yawOffsetDeg)) continue;

                offset.y = 0f;
                scratch.Add((offset.sqrMagnitude, damageable));
            }

            scratch.Sort((a, b) => a.sqr.CompareTo(b.sqr));
            int take = maxTargets > 0 ? Mathf.Min(maxTargets, scratch.Count) : scratch.Count;
            for (int i = 0; i < take; i++) results.Add(scratch[i].target);
        }

        public static void GatherRadius(Vector3 origin, float radius, LayerMask mask, List<IDamageable> results)
            => Gather(origin, Formation.FullCircle(0f, radius), mask, 0, results, 0f);
    }
}
