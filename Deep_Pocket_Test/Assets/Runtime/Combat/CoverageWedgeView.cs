using UnityEngine;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Dibuja en el suelo el abanico que cubre un arma (malla procedural). Tecla C para mostrar/ocultar.
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class CoverageWedgeView : MonoBehaviour
    {
        const float Height = 0.06f;
        const float FlashTime = 0.18f;

        MeshFilter filter;
        MeshRenderer rend;
        Mesh mesh;
        Material mat;
        Color baseColor;
        WeaponManager manager;
        Formation formation;
        bool hasFormation;
        float yaw;
        float flash;

        public void Setup(Color color, WeaponManager weaponManager)
        {
            baseColor = color;
            manager = weaponManager;
            filter = GetComponent<MeshFilter>();
            rend = GetComponent<MeshRenderer>();
            mesh = new Mesh { name = "CoverageWedge" };
            filter.sharedMesh = mesh;
            mat = VisualUtil.Unlit(color);
            rend.sharedMaterial = mat;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
        }

        public void SetFormation(Formation f, float yawDeg)
        {
            yaw = yawDeg;
            if (hasFormation && Approx(f, formation)) return;
            formation = f;
            hasFormation = true;
            Rebuild();
        }

        public void Flash() => flash = FlashTime;

        static bool Approx(Formation a, Formation b)
            => Mathf.Approximately(a.angleStartDeg, b.angleStartDeg) && Mathf.Approximately(a.angleEndDeg, b.angleEndDeg)
            && Mathf.Approximately(a.innerRadius, b.innerRadius) && Mathf.Approximately(a.outerRadius, b.outerRadius);

        void LateUpdate()
        {
            if (rend == null) return;
            rend.enabled = manager == null || manager.ShowCoverage;

            flash = Mathf.Max(0f, flash - Time.unscaledDeltaTime);
            Color c = baseColor;
            c.a = Mathf.Lerp(0.14f, 0.5f, flash / FlashTime);
            mat.color = c;

            Vector3 parent = transform.parent.position;
            transform.position = new Vector3(parent.x, Height, parent.z);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        void Rebuild()
        {
            int steps = Mathf.Max(2, Mathf.CeilToInt(formation.AngleWidthDeg / 6f));
            Vector3[] verts = new Vector3[(steps + 1) * 2];
            Color[] colors = new Color[verts.Length];
            int[] tris = new int[steps * 6];

            for (int i = 0; i <= steps; i++)
            {
                float a = (formation.angleStartDeg + formation.AngleWidthDeg * i / steps) * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a));
                verts[i * 2] = dir * formation.innerRadius;
                verts[i * 2 + 1] = dir * formation.outerRadius;
                colors[i * 2] = Color.white;
                colors[i * 2 + 1] = Color.white;
            }
            for (int i = 0; i < steps; i++)
            {
                int v = i * 2, t = i * 6;
                tris[t] = v; tris[t + 1] = v + 1; tris[t + 2] = v + 2;
                tris[t + 3] = v + 1; tris[t + 4] = v + 3; tris[t + 5] = v + 2;
            }

            mesh.Clear();
            mesh.vertices = verts;
            mesh.colors = colors;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
        }
    }
}
