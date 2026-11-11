using UnityEngine;

namespace DeepPocket.Data
{
    // Todo lo que una habilidad necesita saber para ejecutarse. Lo arma el WeaponController.
    public struct AbilityContext
    {
        public Transform owner;        // el jugador
        public Vector3 origin;         // posición del jugador
        public float baseDamage;       // daño del arma (ya con multiplicadores del jugador)
        public LayerMask enemyMask;
        public Formation formation;    // zona de ataque del arma según su lugar en la grilla
        public float yawOffsetDeg;     // 0 en brújula fija; rotación Y del jugador en brújula relativa
        public object source;          // quién dispara la habilidad (para acreditar kills)
    }

    // Cada habilidad es una clase hija; de cada una se crean tantos assets como se quiera.
    // El asset NO guarda estado (cooldowns, contadores): eso vive en el runtime del arma.
    public abstract class SpecialAbilityDefinition : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        public abstract void Execute(AbilityContext ctx);

        // Zona que alcanza la habilidad (se dibuja en el suelo al dispararse). Por defecto, la zona del arma.
        public virtual Formation GetArea(AbilityContext ctx) => ctx.formation;
    }
}
