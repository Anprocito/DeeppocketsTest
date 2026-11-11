using UnityEngine;
using UnityEngine.UI;

namespace DeepPocket.Runtime
{
    // Anillo radial de cooldown sobre la celda de un arma (o contador "3/5" para triggers por golpes/kills).
    public class CooldownRing : MonoBehaviour
    {
        RuntimeItem item;
        WeaponManager manager;
        Image overlay;
        Text counter;

        public void Bind(RuntimeItem it, WeaponManager mgr, Image fillOverlay, Text counterText)
        {
            item = it;
            manager = mgr;
            overlay = fillOverlay;
            counter = counterText;
        }

        void Update()
        {
            if (manager == null || item == null) return;

            WeaponController wc;
            if (!manager.TryGetController(item, out wc) || !wc.HasSpecial)
            {
                overlay.fillAmount = 0f;
                counter.text = "";
                return;
            }

            if (wc.UsesRadial)
            {
                overlay.fillAmount = 1f - wc.Progress01;   // oscuro = lo que falta recargar
                counter.text = "";
            }
            else
            {
                overlay.fillAmount = 0f;
                counter.text = wc.ProgressLabel;
            }
        }
    }
}
