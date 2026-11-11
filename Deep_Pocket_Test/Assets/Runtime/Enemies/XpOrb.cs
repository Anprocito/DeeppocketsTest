using UnityEngine;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Orbe de experiencia: se acerca al jugador dentro del radio de recolección (stat PickupRadius).
    public class XpOrb : MonoBehaviour
    {
        static PlayerExperience xp;
        static PlayerStats stats;
        static Transform player;

        int amount;

        public static void Configure(PlayerExperience experience, PlayerStats playerStats)
        {
            xp = experience;
            stats = playerStats;
            player = experience != null ? experience.transform : null;
        }

        public static void Spawn(Vector3 position, int amount)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "XpOrb";
            go.transform.position = new Vector3(position.x, 0.4f, position.z);
            go.transform.localScale = Vector3.one * 0.35f;
            Collider col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            go.GetComponent<Renderer>().material = VisualUtil.Unlit(new Color(0.55f, 0.45f, 0.85f));
            go.AddComponent<XpOrb>().amount = amount;
        }

        void Update()
        {
            if (player == null || xp == null || stats == null) return;

            Vector3 to = player.position - transform.position;
            to.y = 0f;
            float dist = to.magnitude;

            if (dist < stats.Get(StatType.PickupRadius))
                transform.position += (to / Mathf.Max(dist, 0.01f)) * 12f * Time.deltaTime;

            if (dist < 0.8f)
            {
                xp.AddXp(amount);
                Destroy(gameObject);
            }
        }
    }
}
