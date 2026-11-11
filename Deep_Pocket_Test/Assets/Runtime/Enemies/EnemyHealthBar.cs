using UnityEngine;

namespace DeepPocket.Runtime
{
    // Barra de vida individual sobre cada enemigo (en el mundo, mirando siempre a la cámara).
    // Es un objeto independiente que sigue al enemigo y se destruye sola cuando este desaparece.
    public class EnemyHealthBar : MonoBehaviour
    {
        const float BaseWidth = 1.0f;
        const float BarHeight = 0.14f;

        Enemy enemy;
        Transform fillPivot;
        Camera cam;
        float width;
        float yOffset;

        public static EnemyHealthBar Attach(Enemy target)
        {
            GameObject root = new GameObject("HealthBar");
            EnemyHealthBar bar = root.AddComponent<EnemyHealthBar>();
            bar.enemy = target;
            bar.width = BaseWidth * Mathf.Max(0.8f, target.VisualScale);
            bar.yOffset = target.VisualScale + 0.45f;   // el centro de la cápsula está a 'scale' del piso
            bar.Build(root.transform);
            return bar;
        }

        void Build(Transform root)
        {
            MakeQuad("Background", root, new Vector3(width + 0.06f, BarHeight + 0.06f, 1f), Vector3.zero,
                     new Color(0.05f, 0.04f, 0.07f, 0.9f), 0);

            fillPivot = new GameObject("FillPivot").transform;
            fillPivot.SetParent(root, false);
            fillPivot.localPosition = new Vector3(-width * 0.5f, 0f, -0.01f);
            fillPivot.localScale = new Vector3(width, BarHeight, 1f);

            // El quad mide 1x1: corrido medio unidad, ocupa de 0 a 1 del pivote (la barra se achica desde la derecha).
            MakeQuad("Fill", fillPivot, Vector3.one, new Vector3(0.5f, 0f, 0f), new Color(0.85f, 0.25f, 0.2f, 1f), 1);
        }

        static void MakeQuad(string name, Transform parent, Vector3 scale, Vector3 localPos, Color color, int order)
        {
            GameObject q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            q.name = name;
            Collider col = q.GetComponent<Collider>();
            if (col != null) Destroy(col);
            q.transform.SetParent(parent, false);
            q.transform.localPosition = localPos;
            q.transform.localScale = scale;

            MeshRenderer r = q.GetComponent<MeshRenderer>();
            r.sharedMaterial = VisualUtil.Unlit(color);
            r.sortingOrder = order;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        void LateUpdate()
        {
            if (enemy == null) { Destroy(gameObject); return; }
            if (cam == null) cam = Camera.main;

            transform.position = enemy.transform.position + Vector3.up * yOffset;
            if (cam != null) transform.rotation = cam.transform.rotation;

            float frac = enemy.HealthFraction;
            fillPivot.localScale = new Vector3(Mathf.Max(0.0001f, width * frac), BarHeight, 1f);
        }
    }
}
