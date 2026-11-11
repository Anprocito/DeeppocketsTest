using UnityEngine;

namespace DeepPocket.Data
{
    public enum DeployableKind { Mine, Decoy, Trap }

    // Se activa al arrastrarlo fuera de la grilla hacia el mapa (Drop-to-Deploy).
    [CreateAssetMenu(menuName = "DeepPocket/Items/Deployable", fileName = "NewDeployable")]
    public class DeployableDefinition : ItemDefinition
    {
        public override ItemKind Kind => ItemKind.Deployable;

        public DeployableKind deployableKind = DeployableKind.Mine;
        [Tooltip("Prefab que se instancia en el suelo (con el componente DeployableInstance).")]
        public GameObject prefab;

        [Min(0.5f)] public float radius = 3f;
        [Tooltip("Mine: daño de la explosión. Trap: no se usa (ver tickDamagePerSecond).")]
        [Min(0f)] public float damage = 46f;
        [Tooltip("Segundos en el mapa. 0 = hasta que se active (mina) o lo destruyan (señuelo).")]
        [Min(0f)] public float duration = 0f;

        [Header("Solo Trap")]
        [Range(0.05f, 1f)] public float slowMultiplier = 0.4f;
        [Min(0f)] public float tickDamagePerSecond = 5f;

        [Header("Solo Decoy")]
        [Tooltip("Vida del señuelo. Con 1, cae al primer golpe de cualquier enemigo.")]
        [Min(1f)] public float health = 1f;
    }
}
