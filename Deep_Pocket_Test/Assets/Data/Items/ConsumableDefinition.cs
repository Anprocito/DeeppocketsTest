using UnityEngine;

namespace DeepPocket.Data
{
    // Se consume solo cuando se cumple la condición, y libera su lugar en la grilla.
    [CreateAssetMenu(menuName = "DeepPocket/Items/Consumable", fileName = "NewConsumable")]
    public class ConsumableDefinition : ItemDefinition
    {
        public override ItemKind Kind => ItemKind.Consumable;

        [Range(0.05f, 0.95f)] public float triggerHealthFraction = 0.35f;
        [Min(0f)] public float healAmount = 34f;
    }
}
