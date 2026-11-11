using System.Collections.Generic;
using UnityEngine;

namespace DeepPocket.Data
{
    // Base de todo lo que ocupa espacio en la grilla (armas, pasivos, desplegables, consumibles).
    public abstract class ItemDefinition : ScriptableObject
    {
        [Header("General")]
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public Color color = Color.white;

        [Header("Forma en la grilla")]
        [Tooltip("Celdas que ocupa la pieza. x = columna, y = fila (hacia abajo). Se normaliza para que la esquina superior izquierda sea (0,0).")]
        [SerializeField] Vector2Int[] cells = { Vector2Int.zero };

        public abstract ItemKind Kind { get; }

        public IReadOnlyList<Vector2Int> Cells => cells;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;

        // Ancho (columnas) y alto (filas) de la caja que contiene la pieza.
        public Vector2Int Size
        {
            get
            {
                int w = 0, h = 0;
                for (int i = 0; i < cells.Length; i++)
                {
                    if (cells[i].x + 1 > w) w = cells[i].x + 1;
                    if (cells[i].y + 1 > h) h = cells[i].y + 1;
                }
                return new Vector2Int(w, h);
            }
        }

        // Para el editor y los tests. En runtime no se debe modificar un asset.
        public void SetCells(params Vector2Int[] newCells)
        {
            cells = (newCells == null || newCells.Length == 0) ? new[] { Vector2Int.zero } : newCells;
            Normalize();
        }

        protected virtual void OnValidate() => Normalize();

        void Normalize()
        {
            if (cells == null || cells.Length == 0) { cells = new[] { Vector2Int.zero }; return; }

            int minX = int.MaxValue, minY = int.MaxValue;
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i].x < minX) minX = cells[i].x;
                if (cells[i].y < minY) minY = cells[i].y;
            }
            if (minX == 0 && minY == 0) return;

            Vector2Int shift = new Vector2Int(minX, minY);
            for (int i = 0; i < cells.Length; i++) cells[i] -= shift;
        }
    }
}
