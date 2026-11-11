using UnityEngine;

namespace DeepPocket.Data
{
    // No ataca: da bonus mientras esté colocado en la grilla.
    [CreateAssetMenu(menuName = "DeepPocket/Items/Passive", fileName = "NewPassive")]
    public class PassiveDefinition : ItemDefinition
    {
        public override ItemKind Kind => ItemKind.Passive;

        public StatModifier[] modifiers;
    }
}
