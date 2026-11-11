using System;
using UnityEngine;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Envoltorio de la grilla para la escena: guarda la config y avisa cuando algo cambia.
    public class PlayerInventory : MonoBehaviour
    {
        public GridConfig Config { get; private set; }
        public InventoryGrid Grid { get; private set; }
        public event Action Changed;

        public void Initialize(GridConfig config)
        {
            Config = config;
            Grid = new InventoryGrid(config.columns, config.rows);
            Grid.Changed += NotifyChanged;
        }

        public RuntimeItem CreateItem(ItemDefinition def) => new RuntimeItem(def);

        // Crea y coloca en el primer lugar libre. Devuelve null si no entra.
        public RuntimeItem AddAutoPlaced(ItemDefinition def)
        {
            RuntimeItem item = new RuntimeItem(def);
            if (Grid.TryFindSpace(def, out Vector2Int anchor) && Grid.Place(item, anchor)) return item;
            return null;
        }

        public void NotifyChanged() => Changed?.Invoke();
    }
}
