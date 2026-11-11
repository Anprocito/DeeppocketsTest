using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Pieza arrastrable. El drag lo gestiona InventoryDragController: acá solo se avisa que empezó.
    public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public RuntimeItem Item { get; private set; }
        CanvasGroup group;

        public static ItemView Create(RuntimeItem item, Transform parent, WeaponManager weapons)
        {
            RectTransform rt = UIFactory.MakeRect("Item_" + item.def.DisplayName, parent);
            UIFactory.SetAnchored(rt, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, GridMetrics.PixelSize(item.def.Size));

            ItemView view = rt.gameObject.AddComponent<ItemView>();
            view.Item = item;
            view.group = rt.gameObject.AddComponent<CanvasGroup>();

            RectTransform first = BuildCells(rt, item.def, true, 1f);

            if (item.def is WeaponDefinition && weapons != null && first != null)
            {
                Image overlay = UIFactory.MakeImage("Cooldown", first, new Color(0, 0, 0, 0.6f), false);
                UIFactory.Stretch(overlay.rectTransform);
                overlay.type = Image.Type.Filled;
                overlay.fillMethod = Image.FillMethod.Radial360;
                overlay.fillOrigin = (int)Image.Origin360.Top;
                overlay.fillClockwise = true;
                overlay.fillAmount = 0f;

                Text counter = UIFactory.MakeText("Counter", first, "", 12, Color.white, TextAnchor.LowerRight);
                UIFactory.Stretch(counter.rectTransform);

                overlay.gameObject.AddComponent<CooldownRing>().Bind(item, weapons, overlay, counter);
            }
            return view;
        }

        // Dibuja una imagen por celda ocupada (así sirven formas que no son rectángulos).
        public static RectTransform BuildCells(RectTransform root, ItemDefinition def, bool showLabel, float alpha)
        {
            RectTransform first = null;
            System.Collections.Generic.IReadOnlyList<Vector2Int> cells = def.Cells;
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int off = cells[i];
                Image img = UIFactory.MakeImage("Cell", root, new Color(def.color.r, def.color.g, def.color.b, alpha), true);
                UIFactory.SetAnchored(img.rectTransform, new Vector2(0, 1), new Vector2(0, 1),
                    new Vector2(off.x * GridMetrics.Pitch, -off.y * GridMetrics.Pitch),
                    new Vector2(GridMetrics.Cell, GridMetrics.Cell));
                if (first == null) first = img.rectTransform;
            }

            if (showLabel && first != null)
            {
                Text t = UIFactory.MakeText("Label", first, def.DisplayName, 13, new Color(0.08f, 0.07f, 0.1f), TextAnchor.MiddleCenter);
                UIFactory.Stretch(t.rectTransform);
                t.resizeTextForBestFit = true;
                t.resizeTextMinSize = 8;
                t.resizeTextMaxSize = 14;
            }
            return first;
        }

        public void SetDim(bool dim) { if (group != null) group.alpha = dim ? 0.35f : 1f; }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (InventoryDragController.Instance != null) InventoryDragController.Instance.BeginDrag(this);
        }

        public void OnDrag(PointerEventData eventData) { }
        public void OnEndDrag(PointerEventData eventData) { }
    }
}
