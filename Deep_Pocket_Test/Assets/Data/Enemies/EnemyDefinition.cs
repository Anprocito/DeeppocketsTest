using UnityEngine;

namespace DeepPocket.Data
{
    [CreateAssetMenu(menuName = "DeepPocket/Enemy", fileName = "NewEnemy")]
    public class EnemyDefinition : ScriptableObject
    {
        public string displayName;
        [Tooltip("Opcional. Si está vacío se genera una cápsula de color automáticamente.")]
        public GameObject prefab;
        public Color color = new Color(0.56f, 0.35f, 0.35f);
        [Min(0.3f)] public float visualScale = 1f;

        [Header("Combate")]
        [Min(1f)] public float maxHealth = 26f;
        [Min(0f)] public float moveSpeed = 2f;
        [Min(0f)] public float contactDamage = 9f;
        [Min(0.1f)] public float contactRange = 1.1f;
        [Min(0.1f)] public float contactCooldown = 0.6f;

        [Header("Recompensas")]
        [Min(0)] public int xpReward = 8;
        [Min(0)] public int goldReward = 2;

        [Header("Escalado por oleada (0.13 = +13% por oleada)")]
        [Min(0f)] public float healthScalePerWave = 0.13f;
        [Min(0f)] public float damageScalePerWave = 0.06f;

        public float HealthAtWave(int wave) => maxHealth * (1f + (Mathf.Max(1, wave) - 1) * healthScalePerWave);
        public float DamageAtWave(int wave) => contactDamage * (1f + (Mathf.Max(1, wave) - 1) * damageScalePerWave);
    }
}
