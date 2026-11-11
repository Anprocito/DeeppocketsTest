using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Mantiene un WeaponController por cada arma que esté colocada en la grilla.
    public class WeaponManager : MonoBehaviour
    {
        PlayerInventory inventory;
        PlayerStats stats;
        PlayerHealth health;
        readonly Dictionary<RuntimeItem, WeaponController> controllers = new Dictionary<RuntimeItem, WeaponController>();
        readonly List<RuntimeItem> toRemove = new List<RuntimeItem>();

        public bool ShowCoverage { get; private set; } = true;

        public void Initialize(PlayerInventory inv, PlayerStats st, PlayerHealth hp)
        {
            inventory = inv;
            stats = st;
            health = hp;
            inventory.Changed += Reconcile;
            Reconcile();
        }

        void OnDestroy()
        {
            if (inventory != null) inventory.Changed -= Reconcile;
        }

        void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.cKey.wasPressedThisFrame) ShowCoverage = !ShowCoverage;
        }

        public bool TryGetController(RuntimeItem item, out WeaponController controller)
            => controllers.TryGetValue(item, out controller) && controller != null;

        void Reconcile()
        {
            IReadOnlyList<RuntimeItem> items = inventory.Grid.Items;
            for (int i = 0; i < items.Count; i++)
            {
                RuntimeItem it = items[i];
                if (it.def is WeaponDefinition && !controllers.ContainsKey(it)) CreateController(it);
            }

            toRemove.Clear();
            foreach (KeyValuePair<RuntimeItem, WeaponController> kv in controllers)
                if (!kv.Key.placed) toRemove.Add(kv.Key);

            for (int i = 0; i < toRemove.Count; i++)
            {
                WeaponController wc = controllers[toRemove[i]];
                if (wc != null) Destroy(wc.gameObject);
                controllers.Remove(toRemove[i]);
            }
        }

        void CreateController(RuntimeItem item)
        {
            GameObject go = new GameObject("Weapon_" + item.def.DisplayName);
            go.transform.SetParent(transform, false);
            WeaponController wc = go.AddComponent<WeaponController>();
            wc.Initialize(item, this, inventory, stats, health, EnemyLayers.Mask);
            controllers[item] = wc;
        }
    }
}
