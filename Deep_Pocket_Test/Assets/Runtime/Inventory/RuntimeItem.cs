using UnityEngine;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Una instancia concreta de un item. La definición (ScriptableObject) es solo lectura;
    // todo lo que cambia durante la partida vive acá o en los controllers.
    public class RuntimeItem
    {
        public readonly ItemDefinition def;
        public Vector2Int anchor;     // esquina superior izquierda dentro de la grilla
        public bool placed;           // está en la grilla
        public bool discarded;        // ya se usó/desplegó: no vuelve

        public RuntimeItem(ItemDefinition definition) { def = definition; }
    }
}
