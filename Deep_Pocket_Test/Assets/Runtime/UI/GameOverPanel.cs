using System;
using UnityEngine;
using UnityEngine.UI;

namespace DeepPocket.Runtime
{
    // Pantalla final: sirve para derrota ("Se cerró el bolsillo") y para victoria.
    public class GameOverPanel : MonoBehaviour
    {
        Text title;
        Text stats;
        Text buttonLabel;

        public static GameOverPanel Build(RectTransform canvas, Action onRestart)
        {
            Image dim = UIFactory.MakeImage("GameOver", canvas, new Color(0, 0, 0, 0.78f), true);
            UIFactory.Stretch(dim.rectTransform);
            GameOverPanel p = dim.gameObject.AddComponent<GameOverPanel>();

            Image box = UIFactory.MakeImage("Box", dim.transform, new Color(0.12f, 0.1f, 0.17f, 1f), true);
            UIFactory.SetAnchored(box.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560, 340));

            p.title = UIFactory.MakeText("Title", box.transform, "", 36, new Color(0.85f, 0.65f, 0.35f), TextAnchor.MiddleCenter);
            UIFactory.SetAnchored(p.title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(520, 50));

            p.stats = UIFactory.MakeText("Stats", box.transform, "", 22, Color.white, TextAnchor.MiddleCenter);
            UIFactory.SetAnchored(p.stats.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(520, 130));

            Button b = UIFactory.MakeButton("Restart", box.transform, new Vector2(240, 56), new Color(0.79f, 0.59f, 0.31f, 1f),
                                            "", 22, () => onRestart(), out p.buttonLabel);
            p.buttonLabel.color = new Color(0.1f, 0.08f, 0.12f);
            UIFactory.SetAnchored((RectTransform)b.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 28), new Vector2(240, 56));

            dim.gameObject.SetActive(false);
            return p;
        }

        static string Clock(float seconds)
        {
            int t = Mathf.FloorToInt(seconds);
            return (t / 60) + ":" + (t % 60).ToString("00");
        }

        // Derrota
        public void Show(int kills, float timeAlive, int level, int wave)
        {
            title.text = "Se cerró el bolsillo";
            title.color = new Color(0.85f, 0.65f, 0.35f);
            buttonLabel.text = "Intentar de nuevo";
            stats.text = "Sobreviviste " + Clock(timeAlive) + " hasta la oleada " + wave
                       + "\nBajas: " + kills + "     Nivel: " + level;
            gameObject.SetActive(true);
        }

        // Victoria
        public void ShowVictory(int kills, float timeAlive, int level, int wave)
        {
            title.text = "¡Victoria!";
            title.color = new Color(0.55f, 0.8f, 0.5f);
            buttonLabel.text = "Jugar de nuevo";
            stats.text = "Aguantaste " + Clock(timeAlive) + " y llegaste a la oleada " + wave
                       + "\nBajas: " + kills + "     Nivel: " + level;
            gameObject.SetActive(true);
        }
    }
}
