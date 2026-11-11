using System.Collections.Generic;
using UnityEngine;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Consume solo los items automáticos (poción) cuando se cumple su condición, y libera la celda.
    public class ConsumableRunner : MonoBehaviour
    {
        PlayerInventory inventory;
        PlayerHealth health;
        float nextCheck;
        readonly List<RuntimeItem> snapshot = new List<RuntimeItem>();

        public void Initialize(PlayerInventory inv, PlayerHealth hp)
        {
            inventory = inv;
            health = hp;
        }

        void Update()
        {
            if (inventory == null || !GameState.IsPlaying || health.IsDead) return;
            if (Time.time < nextCheck) return;
            nextCheck = Time.time + 0.1f;
            if (health.Fraction >= 0.95f) return;

            snapshot.Clear();
            snapshot.AddRange(inventory.Grid.Items);
            for (int i = 0; i < snapshot.Count; i++)
            {
                ConsumableDefinition c = snapshot[i].def as ConsumableDefinition;
                if (c == null || health.Fraction >= c.triggerHealthFraction) continue;

                health.Heal(c.healAmount);
                snapshot[i].discarded = true;
                inventory.Grid.Remove(snapshot[i]);
                break;   // una por chequeo
            }
        }
    }
}
