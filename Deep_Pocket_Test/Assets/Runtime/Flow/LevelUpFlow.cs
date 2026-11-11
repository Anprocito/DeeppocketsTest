using System.Collections.Generic;
using UnityEngine;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Al subir de nivel: pausa -> mejora de estadística -> elegir 1 de 3 items y arrastrarlo a la grilla.
    // Si ninguna de las 3 ofertas entra en la grilla, en vez del item se elige una segunda mejora.
    // Si se ganan varios niveles de golpe, se resuelven uno tras otro. La grilla es editable durante la pausa.
    public class LevelUpFlow : MonoBehaviour
    {
        PlayerExperience xp;
        PlayerStats stats;
        PlayerInventory inventory;
        LootTable loot;
        StatUpgradeDefinition[] upgrades;
        LevelUpPanel statPanel;
        ItemOfferPanel itemPanel;

        int pending;
        bool active;
        RuntimeItem held;
        readonly List<ItemDefinition> offers = new List<ItemDefinition>();

        public bool IsActive => active;

        public void Initialize(PlayerExperience experience, PlayerStats playerStats, PlayerInventory inv, LootTable lootTable,
                               StatUpgradeDefinition[] statUpgrades, LevelUpPanel stat, ItemOfferPanel item)
        {
            xp = experience; stats = playerStats; inventory = inv; loot = lootTable;
            upgrades = statUpgrades; statPanel = stat; itemPanel = item;
            xp.LeveledUp += OnLeveledUp;
            inventory.Changed += OnInventoryChanged;
        }

        void OnDestroy()
        {
            if (xp != null) xp.LeveledUp -= OnLeveledUp;
            if (inventory != null) inventory.Changed -= OnInventoryChanged;
        }

        void OnLeveledUp(int level)
        {
            pending++;
            if (!active) BeginNext();
        }

        void BeginNext()
        {
            if (pending <= 0)
            {
                active = false;
                GameState.Set(GameStateType.Playing);
                return;
            }

            active = true;
            GameState.Set(GameStateType.LevelUp);
            statPanel.Show("Nivel " + xp.Level + ": elegí una mejora", upgrades, OnStatPicked);
        }

        void OnStatPicked(StatUpgradeDefinition upgrade)
        {
            stats.AddPermanent(upgrade.modifier);
            statPanel.Hide();
            StartItemPhase();
        }

        void StartItemPhase()
        {
            offers.Clear();
            if (loot != null) loot.RollDistinct(3, offers);

            if (!AnyFits())
            {
                // Sin espacio: segunda mejora de estadística en lugar del item.
                statPanel.Show("Sin espacio en el bolsillo: elegí una segunda mejora", upgrades, OnBonusStatPicked);
                return;
            }
            itemPanel.ShowOffers(offers, Fits, OnItemPicked);
        }

        void OnBonusStatPicked(StatUpgradeDefinition upgrade)
        {
            stats.AddPermanent(upgrade.modifier);
            statPanel.Hide();
            FinishOne();
        }

        void OnItemPicked(ItemDefinition def)
        {
            if (!Fits(def)) return;
            held = inventory.CreateItem(def);
            itemPanel.ShowHeld(held);   // el juego sigue en pausa hasta que se coloque
        }

        void OnInventoryChanged()
        {
            if (!active) return;

            if (held != null)
            {
                if (held.placed || held.discarded)
                {
                    held = null;
                    itemPanel.Hide();
                    FinishOne();
                }
            }
            else if (itemPanel.IsShowingOffers)
            {
                itemPanel.RefreshFits();   // el jugador pudo hacer lugar reacomodando
            }
        }

        void FinishOne()
        {
            pending--;
            BeginNext();
        }

        bool Fits(ItemDefinition def) => inventory.Grid.TryFindSpace(def, out Vector2Int _);

        bool AnyFits()
        {
            for (int i = 0; i < offers.Count; i++) if (Fits(offers[i])) return true;
            return false;
        }
    }
}
