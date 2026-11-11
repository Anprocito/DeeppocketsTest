using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DeepPocket.Runtime
{
    // Números de daño flotantes. Se dibujan en el canvas, siguiendo el punto del mundo donde ocurrió el golpe.
    // Usan Time.deltaTime, así que se congelan junto con el juego durante las pausas.
    public class DamageNumbers : MonoBehaviour
    {
        const float Life = 0.8f;
        const float RiseSpeed = 1.6f;
        const int MaxActive = 60;

        class Entry
        {
            public Text text;
            public Vector3 world;
            public float age;
        }

        static DamageNumbers instance;

        RectTransform container;
        Camera cam;
        readonly List<Entry> active = new List<Entry>();
        readonly Stack<Entry> pool = new Stack<Entry>();

        public static DamageNumbers Build(RectTransform canvas)
        {
            RectTransform rt = UIFactory.MakeRect("DamageNumbers", canvas);
            UIFactory.Stretch(rt);
            DamageNumbers dn = rt.gameObject.AddComponent<DamageNumbers>();
            dn.container = rt;
            instance = dn;
            return dn;
        }

        public static void Show(Vector3 worldPos, float amount)
        {
            if (instance == null) return;
            instance.Spawn(worldPos, amount);
        }

        void Spawn(Vector3 worldPos, float amount)
        {
            if (active.Count >= MaxActive) return;

            Entry e = pool.Count > 0 ? pool.Pop() : CreateEntry();
            e.age = 0f;
            e.world = worldPos + new Vector3(Random.Range(-0.4f, 0.4f), 0f, Random.Range(-0.2f, 0.2f));
            e.text.text = Mathf.Max(1, Mathf.RoundToInt(amount)).ToString();
            e.text.color = new Color(1f, 0.93f, 0.7f, 1f);
            e.text.gameObject.SetActive(true);
            active.Add(e);
        }

        Entry CreateEntry()
        {
            Text t = UIFactory.MakeText("Dmg", container, "", 30, Color.white, TextAnchor.MiddleCenter);
            t.rectTransform.sizeDelta = new Vector2(140, 40);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            Outline outline = t.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);
            return new Entry { text = t };
        }

        void Update()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            float dt = Time.deltaTime;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                Entry e = active[i];
                e.age += dt;

                if (e.age >= Life)
                {
                    e.text.gameObject.SetActive(false);
                    pool.Push(e);
                    active.RemoveAt(i);
                    continue;
                }

                Vector3 screen = cam.WorldToScreenPoint(e.world + Vector3.up * (e.age * RiseSpeed));
                if (screen.z < 0f) { e.text.enabled = false; continue; }

                e.text.enabled = true;
                e.text.rectTransform.position = screen;   // en un canvas Overlay, la posición de mundo = píxeles de pantalla

                Color c = e.text.color;
                c.a = e.age < Life * 0.5f ? 1f : 1f - (e.age - Life * 0.5f) / (Life * 0.5f);
                e.text.color = c;
            }
        }
    }
}
