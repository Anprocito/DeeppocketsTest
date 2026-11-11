using UnityEngine;

namespace DeepPocket.Data
{
    // Una opción de mejora de estadística al subir de nivel.
    [CreateAssetMenu(menuName = "DeepPocket/Stat Upgrade", fileName = "NewStatUpgrade")]
    public class StatUpgradeDefinition : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public StatModifier modifier;
    }
}
