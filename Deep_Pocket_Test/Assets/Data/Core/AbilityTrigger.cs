using System;
using UnityEngine;

namespace DeepPocket.Data
{
    public enum TriggerType
    {
        Cooldown,            // cada X segundos, aunque no haya enemigos
        EveryNHits,          // cada N ataques básicos que conectan
        EveryNKills,         // cada N enemigos derribados por esta arma
        LowHealth,           // cuando la vida del jugador cae por debajo de un porcentaje
        InventoryCondition   // solo si hay N armas del mismo tipo equipadas
    }

    // Solo se usa el campo que corresponde al tipo elegido; los demás se ignoran.
    [Serializable]
    public class AbilityTrigger
    {
        public TriggerType type = TriggerType.Cooldown;

        [Tooltip("Cooldown: segundos entre activaciones.")]
        [Min(0.1f)] public float cooldownSeconds = 5f;

        [Tooltip("EveryNHits / EveryNKills: cantidad N.")]
        [Min(1)] public int everyN = 5;

        [Tooltip("LowHealth: fracción de vida (0.3 = 30%).")]
        [Range(0.05f, 0.95f)] public float healthFraction = 0.3f;

        [Tooltip("InventoryCondition: copias equipadas necesarias del mismo arma.")]
        [Min(2)] public int requiredCopies = 2;
    }
}
