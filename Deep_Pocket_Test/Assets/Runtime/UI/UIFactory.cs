using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DeepPocket.Runtime
{
    public static class GridMetrics
    {
        public const float Cell = 56f;
        public const float Spacing = 4f;
        public const float Pitch = Cell + Spacing;

        public static Vector2 PixelSize(Vector2Int size)
            => new Vector2(size.x * Pitch - Spacing, size.y * Pitch - Spacing);
    }

    // Helpers para construir uGUI por código (sin TextMeshPro, así no hace falta importar nada).
    public static class UIFactory
    {
        static Font font;
        static Sprite white;

        public static Font DefaultFont
        {
            get
            {
                if (font == null) font = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Helvetica", "Segoe UI" }, 16);
                return font;
            }
        }

        public static Sprite White
        {
            get
            {
                if (white == null)
                {
                    Texture2D t = Texture2D.whiteTexture;
                    white = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
                }
                return white;
            }
        }

        public static RectTransform MakeRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void SetAnchored(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static Image MakeImage(string name, Transform parent, Color color, bool raycast = false)
        {
            RectTransform rt = MakeRect(name, parent);
            Image img = rt.gameObject.AddComponent<Image>();
            img.sprite = White;
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        public static Text MakeText(string name, Transform parent, string text, int size, Color color,
                                    TextAnchor align = TextAnchor.UpperLeft)
        {
            RectTransform rt = MakeRect(name, parent);
            Text t = rt.gameObject.AddComponent<Text>();
            t.font = DefaultFont;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        public static Button MakeButton(string name, Transform parent, Vector2 size, Color color, string label,
                                        int fontSize, UnityAction onClick, out Text labelText)
        {
            Image bg = MakeImage(name, parent, color, true);
            bg.rectTransform.sizeDelta = size;
            Button b = bg.gameObject.AddComponent<Button>();
            b.targetGraphic = bg;
            if (onClick != null) b.onClick.AddListener(onClick);

            labelText = MakeText("Label", bg.transform, label, fontSize, Color.white, TextAnchor.MiddleCenter);
            Stretch(labelText.rectTransform);
            labelText.rectTransform.offsetMin = new Vector2(10, 10);
            labelText.rectTransform.offsetMax = new Vector2(-10, -10);
            return b;
        }
    }
}
