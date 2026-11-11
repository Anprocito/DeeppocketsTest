using System;
using UnityEngine;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Nota: NO implementa IDamageable a propósito, así las armas nunca lo consideran un objetivo.
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField, Min(0f)] float invulnerabilityTime = 0.5f;

        PlayerStats stats;
        float current;
        float lastMax;
        float invulnerableUntil;

        public float Current => current;
        public float Max => stats != null ? stats.Get(StatType.MaxHealth) : 70f;
        public float Fraction => Max > 0f ? current / Max : 0f;
        public bool IsDead { get; private set; }

        public event Action<float, float> Changed;   // (actual, máxima)
        public event Action Died;

        public void Initialize(PlayerStats playerStats)
        {
            stats = playerStats;
            lastMax = Max;
            current = lastMax;
            stats.Changed += OnStatsChanged;
            Changed?.Invoke(current, lastMax);
        }

        void OnDestroy()
        {
            if (stats != null) stats.Changed -= OnStatsChanged;
        }

        // Si sube la vida máxima, sube también la actual por la misma cantidad.
        void OnStatsChanged()
        {
            float max = Max;
            if (Mathf.Approximately(max, lastMax)) return;
            if (max > lastMax) current += max - lastMax;
            current = Mathf.Min(current, max);
            lastMax = max;
            Changed?.Invoke(current, max);
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || Time.time < invulnerableUntil) return;
            invulnerableUntil = Time.time + invulnerabilityTime;
            current = Mathf.Max(0f, current - amount);
            Changed?.Invoke(current, Max);
            if (current <= 0f)
            {
                IsDead = true;
                Died?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            current = Mathf.Min(Max, current + amount);
            Changed?.Invoke(current, Max);
        }
    }
}
