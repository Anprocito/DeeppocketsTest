using UnityEngine;
using UnityEngine.UI;

namespace DeepPocket.Runtime
{
    public class HudView : MonoBehaviour
    {
        GameManager gm;
        PlayerHealth health;
        PlayerExperience xp;
        EnemySpawner spawner;
        Image hpFill, xpFill;
        Text hpText, statusText, waveText;

        public static HudView Build(RectTransform canvas, GameManager gm, PlayerHealth health,
                                    PlayerExperience xp, EnemySpawner spawner)
        {
            Image panel = UIFactory.MakeImage("Hud", canvas, new Color(0, 0, 0, 0.55f), false);
            UIFactory.SetAnchored(panel.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(520, 124));

            HudView hud = panel.gameObject.AddComponent<HudView>();
            hud.gm = gm; hud.health = health; hud.xp = xp; hud.spawner = spawner;

            hud.hpFill = MakeBar(panel.transform, "Vida", -12f, new Color(0.78f, 0.31f, 0.23f));
            hud.xpFill = MakeBar(panel.transform, "Exp", -44f, new Color(0.79f, 0.59f, 0.31f));

            hud.hpText = UIFactory.MakeText("HpText", panel.transform, "", 16, Color.white, TextAnchor.MiddleLeft);
            UIFactory.SetAnchored(hud.hpText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(430, -12), new Vector2(80, 20));

            hud.statusText = UIFactory.MakeText("Status", panel.transform, "", 20, Color.white);
            UIFactory.SetAnchored(hud.statusText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(14, -80), new Vector2(492, 32));

            // Abajo a la izquierda: "Oleada: N"
            hud.waveText = UIFactory.MakeText("WaveLabel", canvas, "Oleada: 1", 44, Color.white, TextAnchor.LowerLeft);
            UIFactory.SetAnchored(hud.waveText.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(28, 22), new Vector2(460, 64));
            Shadow shadow = hud.waveText.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            shadow.effectDistance = new Vector2(2f, -2f);
            return hud;
        }

        static Image MakeBar(Transform parent, string label, float y, Color fillColor)
        {
            Text l = UIFactory.MakeText(label + "Label", parent, label, 16, new Color(0.8f, 0.78f, 0.72f), TextAnchor.MiddleLeft);
            UIFactory.SetAnchored(l.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(14, y), new Vector2(60, 20));

            Image track = UIFactory.MakeImage(label + "Track", parent, new Color(0.08f, 0.07f, 0.1f, 1f), false);
            UIFactory.SetAnchored(track.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(80, y), new Vector2(340, 20));

            Image fill = UIFactory.MakeImage(label + "Fill", track.transform, fillColor, false);
            UIFactory.Stretch(fill.rectTransform);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 1f;
            return fill;
        }

        static string Clock(float seconds)
        {
            int t = Mathf.FloorToInt(seconds);
            return (t / 60) + ":" + (t % 60).ToString("00");
        }

        void Update()
        {
            if (health == null) return;
            hpFill.fillAmount = Mathf.Clamp01(health.Fraction);
            hpText.text = Mathf.CeilToInt(health.Current) + "/" + Mathf.CeilToInt(health.Max);
            xpFill.fillAmount = Mathf.Clamp01(xp.Xp / xp.XpToNext);

            statusText.text = "Nivel " + xp.Level + "   " + Clock(gm.TimeAlive) + " / " + Clock(gm.victoryTimeSeconds)
                            + "   Bajas " + gm.Kills;
            waveText.text = "Oleada: " + spawner.CurrentWave;
        }
    }
}
