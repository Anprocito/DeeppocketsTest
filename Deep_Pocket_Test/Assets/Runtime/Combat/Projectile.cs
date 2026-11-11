using UnityEngine;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    public class Projectile : MonoBehaviour
    {
        const float HitRadius = 0.35f;

        Vector3 direction;
        float speed = 14f;
        float damage;
        float lifetime = 3f;
        LayerMask mask;
        object source;
        readonly Collider[] buffer = new Collider[8];

        public static Projectile Spawn(WeaponDefinition def, Vector3 position, Vector3 direction,
                                       float damage, LayerMask mask, object source)
        {
            GameObject go;
            if (def.projectilePrefab != null)
            {
                go = Instantiate(def.projectilePrefab, position, Quaternion.LookRotation(direction));
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = "Projectile";
                go.transform.position = position;
                go.transform.localScale = Vector3.one * 0.35f;
                Collider col = go.GetComponent<Collider>();
                if (col != null) Destroy(col);
                go.GetComponent<Renderer>().material = VisualUtil.Unlit(def.color);
            }

            Projectile p = go.GetComponent<Projectile>();
            if (p == null) p = go.AddComponent<Projectile>();
            p.direction = direction;
            p.speed = def.projectileSpeed;
            p.damage = damage;
            p.mask = mask;
            p.source = source;
            return p;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            transform.position += direction * speed * dt;
            lifetime -= dt;
            if (lifetime <= 0f) { Destroy(gameObject); return; }

            int count = Physics.OverlapSphereNonAlloc(transform.position, HitRadius, buffer, mask,
                                                      QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                IDamageable target = buffer[i].GetComponentInParent<IDamageable>();
                if (target == null) continue;
                target.TakeDamage(damage, source);
                Destroy(gameObject);
                return;
            }
        }
    }
}
