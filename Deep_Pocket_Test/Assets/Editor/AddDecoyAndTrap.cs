#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using DeepPocket.Data;

namespace DeepPocket.EditorTools
{
    // Menú: DeepPocket > Agregar señuelo y trampa
    // Señuelo: morado, cápsula, 1 de vida, atrae a toda la horda.
    // Trampa: usa el mismo aspecto que la mina (disco con núcleo) pero en verde.
    // Los suma a las LootTable existentes. Se puede ejecutar más de una vez sin duplicar nada.
    public static class AddDecoyAndTrap
    {
        const string ItemsFolder = "Assets/DeepPocket/Data/Samples/Items";

        [MenuItem("DeepPocket/Agregar señuelo y trampa")]
        public static void Run()
        {
            DeployableDefinition decoy = GetOrCreate("Senuelo", out bool decoyNew);
            if (decoyNew)
            {
                decoy.displayName = "Señuelo";
                decoy.description = "Atrae a toda la horda, sin importar la distancia. Tiene 1 de vida.";
                decoy.SetCells(Vector2Int.zero);
                decoy.duration = 0f;   // dura hasta que un enemigo lo golpee
                decoy.radius = 1f;
            }
            decoy.deployableKind = DeployableKind.Decoy;
            decoy.color = new Color(0.62f, 0.35f, 0.85f);   // morado
            decoy.health = 1f;
            EditorUtility.SetDirty(decoy);

            DeployableDefinition trap = GetOrCreate("Trampa", out bool trapNew);
            if (trapNew)
            {
                trap.displayName = "Trampa";
                trap.description = "Frena y daña a los enemigos que pisan su zona.";
                trap.SetCells(Vector2Int.zero);
                trap.radius = 3f;                 // mismo tamaño que la mina
                trap.duration = 5f;
                trap.slowMultiplier = 0.4f;
                trap.tickDamagePerSecond = 5f;
            }
            trap.deployableKind = DeployableKind.Trap;
            trap.color = new Color(0.30f, 0.75f, 0.35f);   // verde
            EditorUtility.SetDirty(trap);

            int updated = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:LootTable"))
            {
                LootTable table = AssetDatabase.LoadAssetAtPath<LootTable>(AssetDatabase.GUIDToAssetPath(guid));
                if (table == null) continue;
                bool changed = AddEntry(table, decoy) | AddEntry(table, trap);
                if (changed) { EditorUtility.SetDirty(table); updated++; }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = decoy;
            Debug.Log("DeepPocket: señuelo y trampa listos (" + updated + " LootTable actualizada/s).");
        }

        static bool AddEntry(LootTable table, ItemDefinition item)
        {
            int old = table.entries == null ? 0 : table.entries.Length;
            for (int i = 0; i < old; i++) if (table.entries[i].item == item) return false;

            LootTable.Entry[] grown = new LootTable.Entry[old + 1];
            for (int i = 0; i < old; i++) grown[i] = table.entries[i];
            grown[old] = new LootTable.Entry { item = item, weight = 2f };
            table.entries = grown;
            return true;
        }

        static DeployableDefinition GetOrCreate(string assetName, out bool created)
        {
            EnsureFolder(ItemsFolder);
            string path = ItemsFolder + "/" + assetName + ".asset";
            DeployableDefinition existing = AssetDatabase.LoadAssetAtPath<DeployableDefinition>(path);
            if (existing != null) { created = false; return existing; }

            DeployableDefinition instance = ScriptableObject.CreateInstance<DeployableDefinition>();
            AssetDatabase.CreateAsset(instance, path);
            created = true;
            return instance;
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
