using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Dibuja la grilla (fondo, brújula arriba, piezas) y traduce posiciones de pantalla a celdas.
    public class InventoryGridView : MonoBehaviour
    {
        static readonly Color Normal = new Color(0.12f, 0.10f, 0.16f, 1f);
        static readonly Color Valid = new Color(0.20f, 0.45f, 0.25f, 1f);
        static readonly Color Invalid = new Color(0.55f, 0.20f, 0.20f, 1f);
        static readonly string[] Compass8 = { "N", "NE", "E", "SE", "S", "SO", "O", "NO" };

        PlayerInventory inventory;
        WeaponManager weapons;
        RectTransform gridRoot;
        Image[,] cellImages;
        readonly List<GameObject> views = new List<GameObject>();

        public static InventoryGridView Build(RectTransform canvas, PlayerInventory inv, WeaponManager wm)
        {
            int cols = inv.Config.columns, rows = inv.Config.rows;
            float gridW = cols * GridMetrics.Pitch - GridMetrics.Spacing;
            float gridH = rows * GridMetrics.Pitch - GridMetrics.Spacing;

            Image panel = UIFactory.MakeImage("GridPanel", canvas, new Color(0.14f, 0.11f, 0.08f, 0.95f), true);
            UIFactory.SetAnchored(panel.rectTransform, new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(-20, 20), new Vector2(gridW + 32, gridH + 56 + 16 + 24));

            InventoryGridView view = panel.gameObject.AddComponent<InventoryGridView>();
            view.inventory = inv;
            view.weapons = wm;

            Text title = UIFactory.MakeText("Title", panel.transform, "Tu bolsillo", 20, new Color(0.85f, 0.65f, 0.35f));
            UIFactory.SetAnchored(title.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(16, -8), new Vector2(300, 26));

            for (int c = 0; c < cols; c++)
            {
                string label = cols == Compass8.Length ? Compass8[c] : Mathf.RoundToInt(c * 360f / cols) + "°";
                Text t = UIFactory.MakeText("Compass" + c, panel.transform, label, 14, new Color(0.65f, 0.6f, 0.55f), TextAnchor.MiddleCenter);
                UIFactory.SetAnchored(t.rectTransform, new Vector2(0, 1), new Vector2(0, 1),
                    new Vector2(16 + c * GridMetrics.Pitch, -34), new Vector2(GridMetrics.Cell, 18));
            }

            RectTransform root = UIFactory.MakeRect("GridRoot", panel.transform);
            UIFactory.SetAnchored(root, new Vector2(0, 1), new Vector2(0, 1), new Vector2(16, -56), new Vector2(gridW, gridH));
            view.gridRoot = root;

            view.cellImages = new Image[cols, rows];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                {
                    Image img = UIFactory.MakeImage("Cell_" + x + "_" + y, root, Normal, false);
                    UIFactory.SetAnchored(img.rectTransform, new Vector2(0, 1), new Vector2(0, 1),
                        new Vector2(x * GridMetrics.Pitch, -y * GridMetrics.Pitch), new Vector2(GridMetrics.Cell, GridMetrics.Cell));
                    view.cellImages[x, y] = img;
                }

            Text hint = UIFactory.MakeText("Hint", panel.transform, "Fila de arriba: pega lejos · fila de abajo: pegado al cuerpo", 13,
                                           new Color(0.65f, 0.6f, 0.55f));
            UIFactory.SetAnchored(hint.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(16, 6), new Vector2(gridW, 22));

            inv.Changed += view.Rebuild;
            view.Rebuild();
            return view;
        }

        void OnDestroy()
        {
            if (inventory != null) inventory.Changed -= Rebuild;
        }

        void Rebuild()
        {
            for (int i = 0; i < views.Count; i++) if (views[i] != null) Destroy(views[i]);
            views.Clear();

            IReadOnlyList<RuntimeItem> items = inventory.Grid.Items;
            for (int i = 0; i < items.Count; i++)
            {
                RuntimeItem item = items[i];
                ItemView v = ItemView.Create(item, gridRoot, weapons);
                ((RectTransform)v.transform).anchoredPosition =
                    new Vector2(item.anchor.x * GridMetrics.Pitch, -item.anchor.y * GridMetrics.Pitch);
                views.Add(v.gameObject);
            }
        }

        // Devuelve true si el puntero está sobre la grilla (con un margen de tolerancia).
        // La pieza se centra en el puntero y se ajusta al borde si se pasa.
        public bool TryGetAnchor(Vector2 screenPos, Vector2Int size, out Vector2Int anchor)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRoot, screenPos, null, out Vector2 local);

            float w = gridRoot.rect.width, h = gridRoot.rect.height;
            Vector2 px = GridMetrics.PixelSize(size);
            float topLeftX = local.x - px.x * 0.5f;
            float topLeftY = local.y + px.y * 0.5f;

            int col = Mathf.RoundToInt(topLeftX / GridMetrics.Pitch);
            int row = Mathf.RoundToInt(-topLeftY / GridMetrics.Pitch);
            col = Mathf.Clamp(col, 0, Mathf.Max(0, inventory.Grid.Columns - size.x));
            row = Mathf.Clamp(row, 0, Mathf.Max(0, inventory.Grid.Rows - size.y));
            anchor = new Vector2Int(col, row);

            float margin = GridMetrics.Pitch * 0.6f;
            return local.x > -margin && local.x < w + margin && local.y < margin && local.y > -(h + margin);
        }

        // Centro de la pieza (en coordenadas de mundo del canvas) si estuviera en 'anchor'.
        public Vector3 AnchorToWorld(Vector2Int anchor, Vector2Int size)
        {
            Vector2 px = GridMetrics.PixelSize(size);
            Vector3 local = new Vector3(anchor.x * GridMetrics.Pitch + px.x * 0.5f,
                                        -(anchor.y * GridMetrics.Pitch + px.y * 0.5f), 0f);
            return gridRoot.TransformPoint(local);
        }

        public void Highlight(ItemDefinition def, Vector2Int anchor, bool valid)
        {
            ClearHighlight();
            IReadOnlyList<Vector2Int> cells = def.Cells;
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int c = anchor + cells[i];
                if (c.x < 0 || c.x >= cellImages.GetLength(0) || c.y < 0 || c.y >= cellImages.GetLength(1)) continue;
                cellImages[c.x, c.y].color = valid ? Valid : Invalid;
            }
        }

        public void ClearHighlight()
        {
            for (int y = 0; y < cellImages.GetLength(1); y++)
                for (int x = 0; x < cellImages.GetLength(0); x++)
                    cellImages[x, y].color = Normal;
        }
    }
}
