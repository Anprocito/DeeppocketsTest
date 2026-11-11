using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    public class EnemySpawner : MonoBehaviour
    {
        WaveDefinition waves;
        Transform player;
        PlayerHealth health;
        float waveTimer;
        float spawnTimer;
        readonly List<Enemy> alive = new List<Enemy>();

        public int CurrentWave { get; private set; } = 1;
        public event Action<int> WaveChanged;

        public void Initialize(WaveDefinition definition, Transform playerTransform, PlayerHealth playerHealth)
        {
            waves = definition;
            player = playerTransform;
            health = playerHealth;
        }

        void Update()
        {
            if (waves == null || player == null || !GameState.IsPlaying || health.IsDead) return;
            float dt = Time.deltaTime;

            waveTimer += dt;
            if (waveTimer >= waves.WaveDurationAt(CurrentWave))
            {
                waveTimer = 0f;
                CurrentWave++;
                WaveChanged?.Invoke(CurrentWave);
            }

            spawnTimer -= dt;
            if (spawnTimer > 0f) return;
            spawnTimer = waves.SpawnIntervalAt(CurrentWave);

            Spawn();
            if (CurrentWave >= waves.extraSpawnFromWave && UnityEngine.Random.value < waves.extraSpawnChance) Spawn();
        }

        void Spawn()
        {
            alive.RemoveAll(e => e == null);
            if (alive.Count >= waves.maxAlive) return;

            EnemyDefinition def = waves.PickEnemy(CurrentWave);
            if (def == null) return;

            float angle = UnityEngine.Random.value * Mathf.PI * 2f;
            Vector3 pos = player.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * waves.spawnRadius;
            pos.y = player.position.y;

            Enemy enemy = Create(def, pos);
            enemy.Init(def, CurrentWave, health, waves.EnemySpeedMultiplierAt(CurrentWave));
            alive.Add(enemy);
        }

        static Enemy Create(EnemyDefinition def, Vector3 pos)
        {
            GameObject go;
            if (def.prefab != null)
            {
                go = Instantiate(def.prefab, pos, Quaternion.identity);
            }
            else
            {
                // Sin prefab: cápsula de color con collider trigger (no empuja al jugador).
                go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = string.IsNullOrEmpty(def.displayName) ? def.name : def.displayName;
                go.transform.localScale = Vector3.one * def.visualScale;
                pos.y = def.visualScale;
                go.transform.position = pos;
                go.GetComponent<Collider>().isTrigger = true;
                VisualUtil.Tint(go, def.color);
            }

            Enemy e = go.GetComponent<Enemy>();
            if (e == null) e = go.AddComponent<Enemy>();
            return e;
        }
    }
}
