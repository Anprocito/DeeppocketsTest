using UnityEngine;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Muestra un instante en el suelo la zona que alcanzó una habilidad especial.
    public class AreaFlash : MonoBehaviour
    {
        float life;
        float duration;
        Color color;
        Material mat;
        Mesh mesh;

        public static void Spawn(Vector3 center, Formation area, float yawDeg, Color color, float duration = 0.45f)
        {
            GameObject go = new GameObject("AreaFlash");
            go.transform.position = new Vector3(center.x, 0.09f, center.z);   // un poco arriba del abanico fijo (0.06)
            go.transform.rotation = Quaternion.Euler(0f, yawDeg, 0f);

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            AreaFlash flash = go.AddComponent<AreaFlash>();

            flash.mesh = BuildMesh(area);
            mf.sharedMesh = flash.mesh;
            flash.mat = VisualUtil.Unlit(color);
            mr.sharedMaterial = flash.mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            flash.duration = duration;
            flash.life = duration;
            flash.color = color;
        }

        static Mesh BuildMesh(Formation f)
        {
            int steps = Mathf.Max(2, Mathf.CeilToInt(f.AngleWidthDeg / 6f));
            Vector3[] verts = new Vector3[(steps + 1) * 2];
            Color[] colors = new Color[verts.Length];
            int[] tris = new int[steps * 6];

            for (int i = 0; i <= steps; i++)
            {
                float a = (f.angleStartDeg + f.AngleWidthDeg * i / steps) * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a));
                verts[i * 2] = dir * f.innerRadius;
                verts[i * 2 + 1] = dir * f.outerRadius;
                colors[i * 2] = Color.white;
                colors[i * 2 + 1] = Color.white;
            }
            for (int i = 0; i < steps; i++)
            {
                int v = i * 2, t = i * 6;
                tris[t] = v; tris[t + 1] = v + 1; tris[t + 2] = v + 2;
                tris[t + 3] = v + 1; tris[t + 4] = v + 3; tris[t + 5] = v + 2;
            }

            Mesh m = new Mesh { name = "AreaFlash" };
            m.vertices = verts;
            m.colors = colors;
            m.triangles = tris;
            m.RecalculateBounds();
            return m;
        }

        void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0f) { Destroy(gameObject); return; }

            Color c = color;
            c.a = Mathf.Lerp(0f, 0.6f, life / duration);
            mat.color = c;
        }

        void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
            if (mat != null) Destroy(mat);
        }
    }
}
