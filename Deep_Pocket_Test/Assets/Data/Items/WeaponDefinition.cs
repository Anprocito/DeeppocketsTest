using UnityEngine;

namespace DeepPocket.Data
{
    [CreateAssetMenu(menuName = "DeepPocket/Items/Weapon", fileName = "NewWeapon")]
    public class WeaponDefinition : ItemDefinition
    {
        public override ItemKind Kind => ItemKind.Weapon;

        [Header("Ataque básico (automático)")]
        [Min(0f)] public float damage = 6f;
        [Min(0.05f)] public float attackInterval = 0.5f;
        [Min(1)] public int maxTargetsPerAttack = 1;

        [Header("A distancia")]
        public bool isRanged;
        [Tooltip("Solo si isRanged. Prefab con un componente Projectile.")]
        public GameObject projectilePrefab;
        [Min(0.5f)] public float projectileSpeed = 14f;

        [Header("Habilidad especial (automática)")]
        public SpecialAbilityDefinition special;
        public AbilityTrigger specialTrigger = new AbilityTrigger();
    }
}
