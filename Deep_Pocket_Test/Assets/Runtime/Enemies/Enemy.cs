using System;
using UnityEngine;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    public class Enemy : MonoBehaviour, IDamageable
    {
        public static event Action<Enemy> Killed;

        EnemyDefinition def;
        PlayerHealth player;
        float health;
        float maxHealth;
        float damage;
        float speed;
        float nextContactTime;
        float slowMultiplier = 1f;
        float slowUntil;
        bool dead;

        public EnemyDefinition Definition => def;
        public float HealthFraction => maxHealth > 0f ? Mathf.Clamp01(health / maxHealth) : 0f;
        public float VisualScale => def != null ? def.visualScale : 1f;

        // speedMultiplier viene de WaveDefinition y es el mismo para todos los tipos de enemigo.
        public void Init(EnemyDefinition definition, int wave, PlayerHealth target, float speedMultiplier)
        {
            def = definition;
            player = target;
            maxHealth = def.HealthAtWave(wave);
            health = maxHealth;
            damage = def.DamageAtWave(wave);
            speed = def.moveSpeed * speedMultiplier;

            int layer = EnemyLayers.Layer;
            if (layer >= 0) VisualUtil.SetLayerRecursive(gameObject, layer);

            EnemyHealthBar.Attach(this);
        }

        public void ApplySlow(float multiplier, float duration)
        {
            slowMultiplier = Mathf.Min(slowMultiplier, multiplier);
            slowUntil = Time.time + duration;
        }

        void Update()
        {
            if (def == null || player == null || player.IsDead) return;

            // Los señuelos atraen a TODA la horda, sin importar la distancia.
            DeployableInstance decoy = DeployableInstance.FindDecoy(transform.position);
            Vector3 goal = decoy != null ? decoy.transform.position : player.transform.position;

            Vector3 toGoal = goal - transform.position;
            toGoal.y = 0f;
            float dist = toGoal.magnitude;

            if (Time.time >= slowUntil) slowMultiplier = 1f;

            if (dist > 0.01f)
            {
                Vector3 dir = toGoal / dist;
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                if (dist > def.contactRange * 0.7f)
                    transform.position += dir * (speed * slowMultiplier) * Time.deltaTime;
            }

            // Al llegar al señuelo lo golpea (con 1 de vida cae de un golpe).
            if (decoy != null && dist <= def.contactRange && Time.time >= nextContactTime)
            {
                nextContactTime = Time.time + def.contactCooldown;
                decoy.Hit(damage);
            }

            // Contacto con el jugador.
            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.magnitude <= def.contactRange && Time.time >= nextContactTime)
            {
                nextContactTime = Time.time + def.contactCooldown;
                player.TakeDamage(damage);
            }
        }

        public void TakeDamage(float amount, object source = null)
        {
            if (dead) return;
            health -= amount;

            // Los daños mínimos continuos (trampa) no muestran número para no saturar la pantalla.
            if (amount >= 1f)
                DamageNumbers.Show(transform.position + Vector3.up * (VisualScale + 0.9f), amount);

            if (health <= 0f) Die(source);
        }

        void Die(object source)
        {
            dead = true;
            Killed?.Invoke(this);
            IKillListener listener = source as IKillListener;
            if (listener != null) listener.OnKill();
            XpOrb.Spawn(transform.position, def.xpReward);
            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            if (def == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, def.contactRange);
        }
    }
}
