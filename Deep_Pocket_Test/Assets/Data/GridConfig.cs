using System.Collections.Generic;
using UnityEngine;

namespace DeepPocket.Data
{
    public enum CompassMode
    {
        FixedToWorld,      // columna = rumbo fijo del mundo (como en el prototipo web)
        RelativeToPlayer   // columna = rumbo relativo a hacia dónde mira el jugador
    }

    [CreateAssetMenu(menuName = "DeepPocket/Grid Config", fileName = "GridConfig")]
    public class GridConfig : ScriptableObject
    {
        [Header("Tamaño de la grilla")]
        [Min(1)] public int columns = 8;
        [Min(1)] public int rows = 4;

        [Header("Alcance")]
        [Tooltip("Radio de la fila de arriba (la más lejana), en unidades de Unity.")]
        [Min(1f)] public float maxRange = 10f;

        [Header("Brújula")]
        public CompassMode compassMode = CompassMode.FixedToWorld;

        public float SectorDegrees => 360f / columns;
        public float RowStep => maxRange / rows;

        public bool InBounds(Vector2Int cell)
            => cell.x >= 0 && cell.x < columns && cell.y >= 0 && cell.y < rows;

        // Convención: x = columna, y = fila (fila 0 = arriba = alcance máximo).
        // Columna 0 = norte (+Z); las columnas crecen en sentido horario.
        // La fila de abajo cubre desde el cuerpo (radio 0) hasta RowStep.
        public Formation ComputeFormation(IReadOnlyList<Vector2Int> cellOffsets, Vector2Int anchor)
        {
            if (cellOffsets == null || cellOffsets.Count == 0) return new Formation(0f, 0f, 0f, 0f);

            int cMin = int.MaxValue, cMax = int.MinValue, rMin = int.MaxValue, rMax = int.MinValue;
            for (int i = 0; i < cellOffsets.Count; i++)
            {
                int col = anchor.x + cellOffsets[i].x;
                int row = anchor.y + cellOffsets[i].y;
                if (col < cMin) cMin = col;
                if (col > cMax) cMax = col;
                if (row < rMin) rMin = row;
                if (row > rMax) rMax = row;
            }

            float sector = SectorDegrees;
            float start = cMin * sector - sector * 0.5f;
            float end = (cMax + 1) * sector - sector * 0.5f;
            float outer = maxRange - rMin * RowStep;
            float inner = Mathf.Max(0f, maxRange - (rMax + 1) * RowStep);
            return new Formation(start, end, inner, outer);
        }
    }
}
