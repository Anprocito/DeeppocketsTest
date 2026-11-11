using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeepPocket.Data
{
    // Tabla con pesos: de acá salen las 3 ofertas de item al subir de nivel.
    [CreateAssetMenu(menuName = "DeepPocket/Loot Table", fileName = "LootTable")]
    public class LootTable : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public ItemDefinition item;
            [Min(0f)] public float weight;
        }

        public Entry[] entries;

        // Sortea 'count' items distintos, sin repetir, respetando los pesos.
        // filter (opcional) permite excluir items.
        public void RollDistinct(int count, List<ItemDefinition> results, Func<ItemDefinition, bool> filter = null)
        {
            results.Clear();
            if (entries == null) return;

            var pool = new List<Entry>();
            for (int i = 0; i < entries.Length; i++)
            {
                Entry e = entries[i];
                if (e.item == null || e.weight <= 0f) continue;
                if (filter != null && !filter(e.item)) continue;
                pool.Add(e);
            }

            while (results.Count < count && pool.Count > 0)
            {
                float total = 0f;
                for (int i = 0; i < pool.Count; i++) total += pool[i].weight;

                float roll = UnityEngine.Random.value * total;
                float acc = 0f;
                int pick = pool.Count - 1;
                for (int i = 0; i < pool.Count; i++)
                {
                    acc += pool[i].weight;
                    if (roll <= acc) { pick = i; break; }
                }

                results.Add(pool[pick].item);
                pool.RemoveAt(pick);
            }
        }

        void OnValidate()
        {
            if (entries == null) return;
            for (int i = 0; i < entries.Length; i++)
                if (entries[i].item == null) Debug.LogWarning($"{name}: la entrada {i} no tiene item asignado.", this);
        }
    }
}
