using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DeepPocket.Runtime
{
    public class GameUI
    {
        public RectTransform canvasRect;
        public HudView hud;
        public InventoryGridView grid;
        public InventoryDragController drag;
        public LevelUpPanel levelUpPanel;
        public ItemOfferPanel offerPanel;
        public GameOverPanel gameOver;
    }

    // Arma todo el canvas por código en runtime: no hay que cablear nada a mano en el editor.
    public static class GameUIBuilder
    {
        public static GameUI Build(GameManager gm, PlayerInventory inventory, WeaponManager weapons,
                                   PlayerHealth health, PlayerExperience xp, EnemySpawner spawner, Action onRestart)
        {
            GameObject canvasGo = new GameObject("GameUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystem();

            RectTransform root = (RectTransform)canvasGo.transform;
            GameUI ui = new GameUI { canvasRect = root };
            DamageNumbers.Build(root);
            ui.grid = InventoryGridView.Build(root, inventory, weapons);
            ui.hud = HudView.Build(root, gm, health, xp, spawner);
            ui.levelUpPanel = LevelUpPanel.Build(root);
            ui.offerPanel = ItemOfferPanel.Build(root, weapons);
            ui.drag = InventoryDragController.Build(root, inventory, ui.grid);
            ui.gameOver = GameOverPanel.Build(root, onRestart);
            return ui;
        }

        static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
    }
}
