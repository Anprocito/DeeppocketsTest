using System;

namespace DeepPocket.Data
{
    public enum StatType { MaxHealth, Damage, MoveSpeed, PickupRadius }

    // Flat: se suma al valor base. PercentAdd: 0.2 = +20% (los porcentajes se suman entre sí).
    public enum ModifierOp { Flat, PercentAdd }

    [Serializable]
    public struct StatModifier
    {
        public StatType stat;
        public ModifierOp op;
        public float value;
    }

    public enum ItemKind { Weapon, Passive, Deployable, Consumable }
}
