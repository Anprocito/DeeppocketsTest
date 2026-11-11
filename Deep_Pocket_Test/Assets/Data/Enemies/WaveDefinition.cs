using System;
using UnityEngine;

namespace DeepPocket.Data
{
    // Toda la progresión de oleadas en un solo asset.
    [CreateAssetMenu(menuName = "DeepPocket/Wave Definition", fileName = "WaveDefinition")]
    public class WaveDefinition : ScriptableObject
    {
        [Serializable]
        public struct SpawnEntry
        {
            public EnemyDefinition enemy;
            [Min(1)] public int firstWave;
            [Min(0f)] public float weight;
        }

        [Header("Qué enemigos aparecen")]
        public SpawnEntry[] spawnTable;

        [Header("Ritmo de aparición")]
        [Min(0.05f)] public float baseSpawnInterval = 1.35f;
        [Tooltip("Se multiplica por oleada: 0.9 = 10% más rápido cada oleada.")]
        [Range(0.5f, 1f)] public float intervalMultiplierPerWave = 0.9f;
        [Min(0.05f)] public float minSpawnInterval = 0.35f;

        [Header("Duración de cada oleada (segundos)")]
        [Min(1f)] public float baseWaveDuration = 20f;
        [Min(0f)] public float durationGrowthPerWave = 1.2f;

        [Header("Límites")]
        [Min(1)] public int maxAlive = 80;
        [Min(1f)] public float spawnRadius = 18f;

        [Header("Aparición doble")]
        [Min(1)] public int extraSpawnFromWave = 3;
        [Range(0f, 1f)] public float extraSpawnChance = 0.3f;

        [Header("Dificultad por oleada")]
        [Tooltip("0.01 = +0.01x de velocidad por oleada, para TODOS los enemigos por igual (la oleada 1 usa la velocidad base).")]
        [Min(0f)] public float enemySpeedScalePerWave = 0.01f;

        public float SpawnIntervalAt(int wave)
            => Mathf.Max(minSpawnInterval, baseSpawnInterval * Mathf.Pow(intervalMultiplierPerWave, Mathf.Max(1, wave) - 1));

        public float WaveDurationAt(int wave) => baseWaveDuration + wave * durationGrowthPerWave;

        public float EnemySpeedMultiplierAt(int wave) => 1f + (Mathf.Max(1, wave) - 1) * enemySpeedScalePerWave;

        public EnemyDefinition PickEnemy(int wave)
        {
            if (spawnTable == null) return null;

            float total = 0f;
            for (int i = 0; i < spawnTable.Length; i++)
                if (spawnTable[i].enemy != null && spawnTable[i].firstWave <= wave) total += spawnTable[i].weight;
            if (total <= 0f) return null;

            float roll = UnityEngine.Random.value * total;
            float acc = 0f;
            EnemyDefinition last = null;
            for (int i = 0; i < spawnTable.Length; i++)
            {
                SpawnEntry e = spawnTable[i];
                if (e.enemy == null || e.firstWave > wave) continue;
                acc += e.weight;
                last = e.enemy;
                if (roll <= acc) return e.enemy;
            }
            return last;
        }
    }
}
