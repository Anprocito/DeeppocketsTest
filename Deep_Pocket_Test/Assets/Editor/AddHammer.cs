#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using DeepPocket.Data;

namespace DeepPocket.EditorTools
{
    // Menú: DeepPocket > Agregar martillo
    // Crea el arma Martillo (2x2, golpe sísmico cada 8 s) y la suma a las LootTable existentes.
    // No modifica ningún otro script ni asset. Se puede ejecutar más de una vez sin duplicar nada.
    public static class AddHammer
    {
        const string ItemsFolder = "Assets/DeepPocket/Data/Samples/Items";
        const string AbilitiesFolder = "Assets/DeepPocket/Data/Samples/Abilities";

        [MenuItem("DeepPocket/Agregar martillo")]
        public static void Run()
        {
            GroundSlamAbility slam = FindFirst<GroundSlamAbility>();
            if (slam == null)
            {
                EnsureFolder(AbilitiesFolder);
                slam = ScriptableObject.CreateInstance<GroundSlamAbility>();
                slam.displayName = "Golpe sísmico";
                slam.description = "Daña todo alrededor del jugador, haya o no enemigos.";
                AssetDatabase.CreateAsset(slam, AbilitiesFolder + "/GroundSlam.asset");
            }

            string hammerPath = ItemsFolder + "/Martillo.asset";
            WeaponDefinition hammer = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(hammerPath);
            if (hammer == null)
            {
                EnsureFolder(ItemsFolder);
                hammer = ScriptableObject.CreateInstance<WeaponDefinition>();
                hammer.displayName = "Martillo";
                hammer.description = "Pesado: ocupa 2x2. Golpe sísmico automático alrededor tuyo.";
                if (ColorUtility.TryParseHtmlString("#e86c6c", out Color c)) hammer.color = c;
                hammer.SetCells(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1));
                hammer.damage = 15f;
                hammer.attackInterval = 1.05f;
                hammer.maxTargetsPerAttack = 8;
                hammer.isRanged = false;
                hammer.special = slam;
                hammer.specialTrigger = new AbilityTrigger { type = TriggerType.Cooldown, cooldownSeconds = 8f };
                AssetDatabase.CreateAsset(hammer, hammerPath);
            }

            int updated = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:LootTable"))
            {
                LootTable table = AssetDatabase.LoadAssetAtPath<LootTable>(AssetDatabase.GUIDToAssetPath(guid));
                if (table == null) continue;

                int old = table.entries == null ? 0 : table.entries.Length;
                bool already = false;
                for (int i = 0; i < old; i++) if (table.entries[i].item == hammer) { already = true; break; }
                if (already) continue;

                LootTable.Entry[] grown = new LootTable.Entry[old + 1];
                for (int i = 0; i < old; i++) grown[i] = table.entries[i];
                grown[old] = new LootTable.Entry { item = hammer, weight = 2f };
                table.entries = grown;
                EditorUtility.SetDirty(table);
                updated++;
            }

            EditorUtility.SetDirty(hammer);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = hammer;
            EditorGUIUtility.PingObject(hammer);
            Debug.Log("DeepPocket: martillo listo (" + updated + " LootTable actualizada/s).");
        }

        static T FindFirst<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            return guids.Length == 0 ? null : AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
#endif
