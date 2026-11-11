using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Maneja todo el drag & drop. Sigue el puntero por polling (no depende de OnEndDrag), así un rebuild
    // de la grilla a mitad del arrastre no puede dejar el estado colgado.
    public class InventoryDragController : MonoBehaviour
    {
        public static InventoryDragController Instance { get; private set; }

        PlayerInventory inventory;
        InventoryGridView gridView;
        RectTransform canvasRect;

        RuntimeItem item;
        ItemView source;
        RectTransform ghost;
        Vector2Int hoverAnchor;
        bool hoverOverGrid;
        bool hoverValid;

        public bool IsDragging => item != null;

        public static InventoryDragController Build(RectTransform canvas, PlayerInventory inv, InventoryGridView grid)
        {
            GameObject go = new GameObject("DragController");
            go.transform.SetParent(canvas, false);
            InventoryDragController c = go.AddComponent<InventoryDragController>();
            c.inventory = inv;
            c.gridView = grid;
            c.canvasRect = canvas;
            Instance = c;
            return c;
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void BeginDrag(ItemView view)
        {
            if (item != null || GameState.Current == GameStateType.GameOver || GameState.Current == GameStateType.Victory) return;

            item = view.Item;
            source = view;
            source.SetDim(true);

            ghost = UIFactory.MakeRect("DragGhost", canvasRect);
            UIFactory.SetAnchored(ghost, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                                  GridMetrics.PixelSize(item.def.Size));
            ItemView.BuildCells(ghost, item.def, true, 0.85f);
            foreach (UnityEngine.UI.Graphic g in ghost.GetComponentsInChildren<UnityEngine.UI.Graphic>())
                g.raycastTarget = false;
            ghost.SetAsLastSibling();

            Pointer p = Pointer.current;
            if (p != null) Move(p.position.ReadValue());
        }

        void Update()
        {
            if (item == null) return;

            Pointer p = Pointer.current;
            if (p == null) { Cancel(); return; }

            Vector2 pos = p.position.ReadValue();
            Move(pos);
            if (!p.press.isPressed) End(pos);
        }

        void Move(Vector2 screenPos)
        {
            Vector2Int size = item.def.Size;
            hoverOverGrid = gridView.TryGetAnchor(screenPos, size, out hoverAnchor);

            if (hoverOverGrid)
            {
                hoverValid = inventory.Grid.CanPlace(item.def, hoverAnchor, item.placed ? item : null);
                gridView.Highlight(item.def, hoverAnchor, hoverValid);
                ghost.position = gridView.AnchorToWorld(hoverAnchor, size);
            }
            else
            {
                gridView.ClearHighlight();
                ghost.position = screenPos;
            }
        }

        void End(Vector2 screenPos)
        {
            if (item.discarded)
            {
                Cancel();
                return;
            }

            if (hoverOverGrid)
            {
                if (hoverValid) inventory.Grid.Place(item, hoverAnchor);   // colocar o mover
            }
            else
            {
                DeployableDefinition deployable = item.def as DeployableDefinition;
                bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                if (deployable != null && !overUi && DeployDropZone.TryGetPoint(Camera.main, screenPos, 0f, out Vector3 point))
                {
                    RuntimeItem used = item;
                    used.discarded = true;
                    if (used.placed) inventory.Grid.Remove(used);   // libera el espacio en la grilla
                    DeployableInstance.Spawn(deployable, point, EnemyLayers.Mask);
                    inventory.NotifyChanged();
                }
                // cualquier otro caso: la pieza vuelve a donde estaba
            }
            Cancel();
        }

        void Cancel()
        {
            gridView.ClearHighlight();
            if (ghost != null) Destroy(ghost.gameObject);
            if (source != null) source.SetDim(false);
            ghost = null;
            source = null;
            item = null;
        }
    }

    // Convierte una posición de pantalla en un punto del suelo (plano horizontal a altura groundY).
    public static class DeployDropZone
    {
        public static bool TryGetPoint(Camera cam, Vector2 screenPos, float groundY, out Vector3 point)
        {
            point = default;
            if (cam == null) return false;
            Ray ray = cam.ScreenPointToRay(screenPos);
            Plane ground = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
            if (!ground.Raycast(ray, out float t)) return false;
            point = ray.GetPoint(t);
            return true;
        }
    }
}
