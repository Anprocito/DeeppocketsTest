using System.Collections.Generic;
using UnityEngine;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Mina, señuelo o trampa ya colocados en el mapa (Drop-to-Deploy).
    // Sin prefab: mina y trampa son un disco con un núcleo (se diferencian por el color del asset);
    // el señuelo es una cápsula del color del asset.
    public class DeployableInstance : MonoBehaviour
    {
        static readonly List<DeployableInstance> decoys = new List<DeployableInstance>();

        DeployableDefinition def;
        LayerMask mask;
        float life;
        float health;
        readonly Collider[] buffer = new Collider[64];

        // Necesario si tenés desactivado el "Domain Reload" al entrar a Play Mode.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => decoys.Clear();

        public static DeployableInstance Spawn(DeployableDefinition definition, Vector3 position, LayerMask enemyMask)
        {
            GameObject go;
            if (definition.prefab != null) go = Instantiate(definition.prefab, position, Quaternion.identity);
            else if (definition.deployableKind == DeployableKind.Decoy) go = CreateDecoyVisual(definition, position);
            else go = CreateAreaVisual(definition, position);

            DeployableInstance inst = go.GetComponent<DeployableInstance>();
            if (inst == null) inst = go.AddComponent<DeployableInstance>();
            inst.def = definition;
            inst.mask = enemyMask;
            inst.life = definition.duration;
            inst.health = definition.health;

            if (definition.deployableKind == DeployableKind.Decoy && !decoys.Contains(inst)) decoys.Add(inst);
            return inst;
        }

        static GameObject CreateDecoyVisual(DeployableDefinition definition, Vector3 position)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Deployable_" + definition.DisplayName;
            go.transform.localScale = Vector3.one * 0.8f;
            go.transform.position = new Vector3(position.x, 0.8f, position.z);   // la cápsula apoya en el piso
            Collider col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            VisualUtil.Tint(go, definition.color);
            return go;
        }

        static GameObject CreateAreaVisual(DeployableDefinition definition, Vector3 position)
        {
            GameObject go = new GameObject("Deployable_" + definition.DisplayName);
            go.transform.position = position;
            float d = definition.radius * 2f;
            MakeDisc(go.transform, "Area", new Vector3(d, 0.02f, d), 0.03f,
                     new Color(definition.color.r, definition.color.g, definition.color.b, 0.22f));
            MakeDisc(go.transform, "Core", new Vector3(0.7f, 0.08f, 0.7f), 0.06f, definition.color);
            return go;
        }

        static void MakeDisc(Transform parent, string name, Vector3 scale, float y, Color color)
        {
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = name;
            disc.transform.SetParent(parent, false);
            disc.transform.localScale = scale;
            disc.transform.localPosition = new Vector3(0f, y, 0f);
            Collider col = disc.GetComponent<Collider>();
            if (col != null) Destroy(col);
            disc.GetComponent<Renderer>().material = VisualUtil.Unlit(color);
        }

        // Señuelo más cercano, SIN límite de distancia: atrae a toda la horda. Devuelve null si no hay.
        public static DeployableInstance FindDecoy(Vector3 from)
        {
            DeployableInstance best = null;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < decoys.Count; i++)
            {
                DeployableInstance d = decoys[i];
                if (d == null) continue;
                float sqr = (d.transform.position - from).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = d; }
            }
            return best;
        }

        // Golpe de un enemigo al señuelo. (A propósito no es IDamageable: las armas del jugador nunca lo apuntan.)
        public void Hit(float amount)
        {
            health -= amount;
            if (health <= 0f) Destroy(gameObject);
        }

        void OnDestroy() => decoys.Remove(this);

        void Update()
        {
            if (def == null) return;

            if (def.duration > 0f)
            {
                life -= Time.deltaTime;
                if (life <= 0f) { Destroy(gameObject); return; }
            }

            if (def.deployableKind == DeployableKind.Mine) TickMine();
            else if (def.deployableKind == DeployableKind.Trap) TickTrap();
        }

        void TickMine()
        {
            Vector3 p = transform.position;
            int count = Physics.OverlapSphereNonAlloc(p, def.radius * 0.4f, buffer, mask, QueryTriggerInteraction.Collide);
            bool triggered = false;
            for (int i = 0; i < count; i++)
                if (buffer[i].GetComponentInParent<IDamageable>() != null) { triggered = true; break; }
            if (!triggered) return;

            count = Physics.OverlapSphereNonAlloc(p, def.radius, buffer, mask, QueryTriggerInteraction.Collide);
            HashSet<IDamageable> hit = new HashSet<IDamageable>();
            for (int i = 0; i < count; i++)
            {
                IDamageable d = buffer[i].GetComponentInParent<IDamageable>();
                if (d != null && hit.Add(d)) d.TakeDamage(def.damage, null);
            }
            Destroy(gameObject);
        }

        void TickTrap()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, def.radius, buffer, mask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                Enemy e = buffer[i].GetComponentInParent<Enemy>();
                if (e == null) continue;
                e.ApplySlow(def.slowMultiplier, 0.25f);
                if (def.tickDamagePerSecond > 0f) e.TakeDamage(def.tickDamagePerSecond * Time.deltaTime, null);
            }
        }
    }
}
