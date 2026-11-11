using UnityEngine;

namespace DeepPocket.Runtime
{
    // Si existe el layer "Enemy" se usa; si no, se consulta contra todo y se filtra por IDamageable.
    public static class EnemyLayers
    {
        public const string LayerName = "Enemy";
        public static int Layer => LayerMask.NameToLayer(LayerName);

        public static LayerMask Mask
        {
            get
            {
                int layer = Layer;
                return layer >= 0 ? (LayerMask)(1 << layer) : (LayerMask)(~0);
            }
        }
    }
}
