using UnityEngine;

namespace DeepPocket.Data
{
    // Zona de ataque de un arma: un sector angular (en grados, 0 = norte = +Z, sentido horario)
    // combinado con una banda de distancia (radio interno y externo). C# puro, testeable.
    public readonly struct Formation
    {
        public readonly float angleStartDeg;
        public readonly float angleEndDeg;
        public readonly float innerRadius;
        public readonly float outerRadius;

        public Formation(float angleStartDeg, float angleEndDeg, float innerRadius, float outerRadius)
        {
            this.angleStartDeg = angleStartDeg;
            this.angleEndDeg = angleEndDeg;
            this.innerRadius = innerRadius;
            this.outerRadius = outerRadius;
        }

        public float AngleWidthDeg => angleEndDeg - angleStartDeg;

        public static Formation FullCircle(float innerRadius, float outerRadius)
            => new Formation(-180f, 180f, innerRadius, outerRadius);

        // offset = posición del objetivo menos la del jugador. slack da un margen de tolerancia en unidades.
        // yawOffsetDeg sirve para el modo "brújula relativa al jugador" (pasar la rotación Y del jugador).
        public bool Contains(Vector3 offset, float slack = 0.3f, float yawOffsetDeg = 0f)
        {
            offset.y = 0f;
            float dist = offset.magnitude;
            if (dist < innerRadius - slack || dist > outerRadius + slack) return false;
            if (dist < 0.0001f) return true;

            float angle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg - yawOffsetDeg;
            return InSector(angle, angleStartDeg, angleEndDeg);
        }

        // Prueba el ángulo, +360 y -360 para resolver el "wraparound" de la brújula (columna 7 pegada a la 0).
        public static bool InSector(float angleDeg, float startDeg, float endDeg)
        {
            const float eps = 0.01f;
            if (angleDeg >= startDeg - eps && angleDeg <= endDeg + eps) return true;
            float up = angleDeg + 360f;
            if (up >= startDeg - eps && up <= endDeg + eps) return true;
            float down = angleDeg - 360f;
            return down >= startDeg - eps && down <= endDeg + eps;
        }
    }
}
