using UnityEngine;

namespace DeepPocket.Data
{
    // Personaje jugable. Solo tiene efectos pasivos: no agrega botones ni habilidades activas.
    [CreateAssetMenu(menuName = "DeepPocket/Character", fileName = "NewCharacter")]
    public class CharacterDefinition : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Valores base")]
        [Min(1f)] public float baseMaxHealth = 70f;
        [Min(0.1f)] public float baseMoveSpeed = 5f;
        [Min(0.1f)] public float baseDamageMultiplier = 1f;
        [Min(0f)] public float basePickupRadius = 3f;
        [Min(0f)] public float goldMultiplier = 1f;

        [Header("Bonus pasivos del personaje")]
        public StatModifier[] startingModifiers;

        [Header("Equipo inicial (se coloca en la grilla al empezar)")]
        public ItemDefinition[] startingItems;
    }
}
