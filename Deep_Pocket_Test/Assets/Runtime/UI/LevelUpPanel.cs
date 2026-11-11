using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Panel de elección de mejora de estadística (hasta 3 opciones).
    public class LevelUpPanel : MonoBehaviour
    {
        const int Slots = 3;
        Text title;
        readonly Button[] buttons = new Button[Slots];
        readonly Text[] labels = new Text[Slots];

        public static LevelUpPanel Build(RectTransform canvas)
        {
            Image panel = UIFactory.MakeImage("LevelUpPanel", canvas, new Color(0.10f, 0.08f, 0.14f, 0.96f), true);
            UIFactory.SetAnchored(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                                  new Vector2(-260, 40), new Vector2(760, 320));
            LevelUpPanel p = panel.gameObject.AddComponent<LevelUpPanel>();

            p.title = UIFactory.MakeText("Title", panel.transform, "", 26, new Color(0.85f, 0.65f, 0.35f));
            UIFactory.SetAnchored(p.title.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -18), new Vector2(720, 40));

            for (int i = 0; i < Slots; i++)
            {
                Text label;
                Button b = UIFactory.MakeButton("Option" + i, panel.transform, new Vector2(220, 210),
                                                new Color(0.25f, 0.2f, 0.33f, 1f), "", 20, null, out label);
                UIFactory.SetAnchored((RectTransform)b.transform, new Vector2(0, 1), new Vector2(0, 1),
                                      new Vector2(20 + i * 240, -80), new Vector2(220, 210));
                p.buttons[i] = b;
                p.labels[i] = label;
            }
            panel.gameObject.SetActive(false);
            return p;
        }

        public void Show(string titleText, IReadOnlyList<StatUpgradeDefinition> options, Action<StatUpgradeDefinition> onPick)
        {
            gameObject.SetActive(true);
            title.text = titleText;
            for (int i = 0; i < Slots; i++)
            {
                buttons[i].onClick.RemoveAllListeners();
                if (options == null || i >= options.Count) { buttons[i].gameObject.SetActive(false); continue; }

                StatUpgradeDefinition option = options[i];
                buttons[i].gameObject.SetActive(true);
                labels[i].text = option.displayName + "\n\n" + option.description;
                buttons[i].onClick.AddListener(() => onPick(option));
            }
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
