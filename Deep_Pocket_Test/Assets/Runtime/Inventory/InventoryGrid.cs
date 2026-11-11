using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Modelo puro de la grilla (sin MonoBehaviour): se puede testear sin abrir la escena.
    public class InventoryGrid
    {
        public int Columns { get; }
        public int Rows { get; }
        public IReadOnlyList<RuntimeItem> Items => items;
        public event Action Changed;

        readonly RuntimeItem[,] occupancy;
        readonly List<RuntimeItem> items = new List<RuntimeItem>();

        public InventoryGrid(int columns, int rows)
        {
            Columns = columns;
            Rows = rows;
            occupancy = new RuntimeItem[columns, rows];
        }

        public bool InBounds(Vector2Int c) => c.x >= 0 && c.x < Columns && c.y >= 0 && c.y < Rows;

        public RuntimeItem GetAt(Vector2Int c) => InBounds(c) ? occupancy[c.x, c.y] : null;

        // 'ignore' sirve para mover un item sin que choque consigo mismo.
        public bool CanPlace(ItemDefinition def, Vector2Int anchor, RuntimeItem ignore = null)
        {
            IReadOnlyList<Vector2Int> cells = def.Cells;
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int c = anchor + cells[i];
                if (!InBounds(c)) return false;
                RuntimeItem occ = occupancy[c.x, c.y];
                if (occ != null && occ != ignore) return false;
            }
            return true;
        }

        // ¿Hay algún lugar donde entre esta forma? (no es lo mismo que contar celdas libres)
        public bool TryFindSpace(ItemDefinition def, out Vector2Int anchor)
        {
            for (int y = 0; y < Rows; y++)
                for (int x = 0; x < Columns; x++)
                {
                    Vector2Int a = new Vector2Int(x, y);
                    if (CanPlace(def, a)) { anchor = a; return true; }
                }
            anchor = default;
            return false;
        }

        // Coloca un item nuevo o mueve uno ya colocado.
        public bool Place(RuntimeItem item, Vector2Int anchor)
        {
            if (item == null || item.discarded) return false;
            if (!CanPlace(item.def, anchor, item)) return false;

            if (item.placed) ClearCells(item);

            IReadOnlyList<Vector2Int> cells = item.def.Cells;
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int c = anchor + cells[i];
                occupancy[c.x, c.y] = item;
            }
            item.anchor = anchor;
            item.placed = true;
            if (!items.Contains(item)) items.Add(item);

            Changed?.Invoke();
            return true;
        }

        public bool Remove(RuntimeItem item)
        {
            if (item == null || !item.placed) return false;
            ClearCells(item);
            items.Remove(item);
            item.placed = false;
            Changed?.Invoke();
            return true;
        }

        public int CountOf(ItemDefinition def)
        {
            int n = 0;
            for (int i = 0; i < items.Count; i++) if (items[i].def == def) n++;
            return n;
        }

        void ClearCells(RuntimeItem item)
        {
            IReadOnlyList<Vector2Int> cells = item.def.Cells;
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int c = item.anchor + cells[i];
                if (InBounds(c) && occupancy[c.x, c.y] == item) occupancy[c.x, c.y] = null;
            }
        }
    }
}
