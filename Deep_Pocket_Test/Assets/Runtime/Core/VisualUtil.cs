using UnityEngine;

namespace DeepPocket.Runtime
{
    public static class VisualUtil
    {
        // Material sin iluminación con soporte de transparencia (sirve en Built-in y en URP).
        public static Material Unlit(Color color)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            Material m = new Material(shader);
            m.color = color;
            return m;
        }

        public static void Tint(GameObject go, Color color)
        {
            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.material.color = color;
        }

        public static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform) SetLayerRecursive(child.gameObject, layer);
        }
    }
}
