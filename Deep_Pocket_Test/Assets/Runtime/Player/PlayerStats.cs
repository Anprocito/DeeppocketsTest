using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Valor final = (base + suma de Flat) * (1 + suma de PercentAdd).
    // Modificadores permanentes (personaje + mejoras de level-up) y de pasivos (mientras estén en la grilla).
    public class PlayerStats : MonoBehaviour
    {
        CharacterDefinition character;
        PlayerInventory inventory;
        readonly List<StatModifier> permanent = new List<StatModifier>();
        readonly List<StatModifier> passive = new List<StatModifier>();

        public event Action Changed;
        public float GoldMultiplier => character != null ? character.goldMultiplier : 1f;

        public void Initialize(CharacterDefinition def, PlayerInventory inv)
        {
            character = def;
            inventory = inv;
            permanent.Clear();
            if (def != null && def.startingModifiers != null) permanent.AddRange(def.startingModifiers);
            inventory.Changed += RecalculatePassives;
            RecalculatePassives();
        }

        void OnDestroy()
        {
            if (inventory != null) inventory.Changed -= RecalculatePassives;
        }

        public void AddPermanent(StatModifier modifier)
        {
            permanent.Add(modifier);
            Changed?.Invoke();
        }

        public float Get(StatType stat)
        {
            float flat = 0f, percent = 0f;
            Accumulate(permanent, stat, ref flat, ref percent);
            Accumulate(passive, stat, ref flat, ref percent);
            return Mathf.Max(0f, (Base(stat) + flat) * (1f + percent));
        }

        static void Accumulate(List<StatModifier> list, StatType stat, ref float flat, ref float percent)
        {
            for (int i = 0; i < list.Count; i++)
            {
                StatModifier m = list[i];
                if (m.stat != stat) continue;
                if (m.op == ModifierOp.Flat) flat += m.value; else percent += m.value;
            }
        }

        float Base(StatType stat)
        {
            switch (stat)
            {
                case StatType.MaxHealth:    return character != null ? character.baseMaxHealth : 70f;
                case StatType.Damage:       return character != null ? character.baseDamageMultiplier : 1f;
                case StatType.MoveSpeed:    return character != null ? character.baseMoveSpeed : 5f;
                case StatType.PickupRadius: return character != null ? character.basePickupRadius : 3f;
                default: return 0f;
            }
        }

        void RecalculatePassives()
        {
            passive.Clear();
            IReadOnlyList<RuntimeItem> items = inventory.Grid.Items;
            for (int i = 0; i < items.Count; i++)
            {
                PassiveDefinition p = items[i].def as PassiveDefinition;
                if (p != null && p.modifiers != null) passive.AddRange(p.modifiers);
            }
            Changed?.Invoke();
        }
    }
}
