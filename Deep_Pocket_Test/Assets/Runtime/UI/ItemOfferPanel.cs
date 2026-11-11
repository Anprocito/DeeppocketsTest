using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Muestra las 3 ofertas de item. Las que no entran en la grilla quedan deshabilitadas (y se reevalúan
    // en vivo). Al elegir una, el item queda "en la mano" hasta que se lo arrastra a la grilla.
    public class ItemOfferPanel : MonoBehaviour
    {
        const int Slots = 3;
        Text title;
        readonly Button[] buttons = new Button[Slots];
        readonly Text[] labels = new Text[Slots];
        RectTransform heldArea;
        ItemView heldView;
        WeaponManager weapons;

        IReadOnlyList<ItemDefinition> current;
        Func<ItemDefinition, bool> fits;

        public bool IsShowingOffers { get; private set; }

        public static ItemOfferPanel Build(RectTransform canvas, WeaponManager wm)
        {
            Image panel = UIFactory.MakeImage("ItemOfferPanel", canvas, new Color(0.10f, 0.08f, 0.14f, 0.96f), true);
            UIFactory.SetAnchored(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                                  new Vector2(-260, 40), new Vector2(760, 360));
            ItemOfferPanel p = panel.gameObject.AddComponent<ItemOfferPanel>();
            p.weapons = wm;

            p.title = UIFactory.MakeText("Title", panel.transform, "", 26, new Color(0.85f, 0.65f, 0.35f));
            UIFactory.SetAnchored(p.title.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -18), new Vector2(720, 40));

            for (int i = 0; i < Slots; i++)
            {
                Text label;
                Button b = UIFactory.MakeButton("Offer" + i, panel.transform, new Vector2(220, 260),
                                                new Color(0.25f, 0.2f, 0.33f, 1f), "", 18, null, out label);
                UIFactory.SetAnchored((RectTransform)b.transform, new Vector2(0, 1), new Vector2(0, 1),
                                      new Vector2(20 + i * 240, -80), new Vector2(220, 260));
                p.buttons[i] = b;
                p.labels[i] = label;
            }

            p.heldArea = UIFactory.MakeRect("HeldArea", panel.transform);
            UIFactory.SetAnchored(p.heldArea, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(10, 10));

            panel.gameObject.SetActive(false);
            return p;
        }

        public void ShowOffers(IReadOnlyList<ItemDefinition> offers, Func<ItemDefinition, bool> fitsFunc,
                               Action<ItemDefinition> onPick)
        {
            gameObject.SetActive(true);
            IsShowingOffers = true;
            current = offers;
            fits = fitsFunc;
            title.text = "Elegí un item para tu bolsillo";

            for (int i = 0; i < Slots; i++)
            {
                buttons[i].onClick.RemoveAllListeners();
                if (i >= offers.Count) { buttons[i].gameObject.SetActive(false); continue; }

                ItemDefinition def = offers[i];
                buttons[i].gameObject.SetActive(true);
                buttons[i].onClick.AddListener(() => { if (fits(def)) onPick(def); });

                Image bg = (Image)buttons[i].targetGraphic;
                bg.color = Color.Lerp(def.color, Color.black, 0.55f);
            }
            RefreshFits();
        }

        // Reevalúa qué ofertas entran (se llama cuando la grilla cambia durante la pausa).
        public void RefreshFits()
        {
            if (!IsShowingOffers || current == null) return;
            for (int i = 0; i < Slots && i < current.Count; i++)
            {
                ItemDefinition def = current[i];
                bool ok = fits(def);
                Vector2Int size = def.Size;
                labels[i].text = def.DisplayName + "\n" + KindLabel(def.Kind) + "  " + size.x + "x" + size.y
                               + "\n\n" + def.description + (ok ? "" : "\n\n(No entra en tu bolsillo)");
                buttons[i].interactable = ok;
            }
        }

        public void ShowHeld(RuntimeItem item)
        {
            IsShowingOffers = false;
            for (int i = 0; i < Slots; i++) buttons[i].gameObject.SetActive(false);
            title.text = "Arrastrá el item a tu inventario";

            if (heldView != null) Destroy(heldView.gameObject);
            heldView = ItemView.Create(item, heldArea, weapons);

            // La pieza usa pivote arriba-izquierda: la corremos para que quede centrada.
            Vector2 px = GridMetrics.PixelSize(item.def.Size);
            RectTransform rt = (RectTransform)heldView.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-px.x * 0.5f, px.y * 0.5f);
        }

        public void Hide()
        {
            IsShowingOffers = false;
            if (heldView != null) Destroy(heldView.gameObject);
            heldView = null;
            gameObject.SetActive(false);
        }

        static string KindLabel(ItemKind k)
        {
            switch (k)
            {
                case ItemKind.Weapon: return "Arma";
                case ItemKind.Passive: return "Pasivo";
                case ItemKind.Deployable: return "Desplegable";
                default: return "Consumible";
            }
        }
    }
}
