#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DeepPocket.EditorTools
{
    // Menú: DeepPocket > Reparar scripts rotos
    // Cuando borrás un script y agregás uno nuevo con el mismo nombre, Unity le asigna otro GUID y los assets
    // y componentes que apuntaban al viejo quedan con "Missing Script". Esta herramienta busca esas referencias
    // rotas y las reapunta al script nuevo, conservando todos los valores que ya tenías.
    // Reconoce: EnemyDefinition, ExtendAbility, RingAbility, BurstAbility, GroundSlamAbility y GameManager.
    public static class RepairBrokenScripts
    {
        static readonly Regex Block = new Regex(@"^--- !u!\d+ &\d+.*?(?=^--- !u!|\z)", RegexOptions.Multiline | RegexOptions.Singleline);
        static readonly Regex ScriptRef = new Regex(@"(m_Script: \{fileID: 11500000, guid: )([0-9a-f]{32})(, type: 3\})");
        static readonly Dictionary<string, string> guidCache = new Dictionary<string, string>();

        [MenuItem("DeepPocket/Reparar scripts rotos")]
        public static void Run()
        {
            if (!EditorUtility.DisplayDialog("Deep Pocket",
                    "Va a editar archivos .asset y .unity de tu proyecto para reapuntar scripts rotos.\n\n" +
                    "Antes: guardá todo, abrí una escena vacía (o cerrá la del juego) y hacé una copia de la carpeta del proyecto.\n\n¿Seguir?",
                    "Reparar", "Cancelar"))
                return;

            guidCache.Clear();
            int fixedRefs = 0;
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles("Assets", "*.asset", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories));

            foreach (string path in files)
            {
                string text = File.ReadAllText(path);
                if (!text.Contains("m_Script:")) continue;

                string fileName = Path.GetFileNameWithoutExtension(path);
                int count = 0;
                string result = Block.Replace(text, m =>
                {
                    string block = m.Value;
                    Match sm = ScriptRef.Match(block);
                    if (!sm.Success) return block;

                    string oldGuid = sm.Groups[2].Value;
                    if (!string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(oldGuid))) return block;   // no está rota

                    string typeName = GuessType(block, fileName);
                    if (typeName == null) return block;

                    string newGuid = ScriptGuid(typeName);
                    if (newGuid == null) return block;

                    count++;
                    return ScriptRef.Replace(block, "${1}" + newGuid + "${3}", 1);
                });

                if (count == 0) continue;
                File.WriteAllText(path, result, new UTF8Encoding(false));
                fixedRefs += count;
                Debug.Log("DeepPocket: reparado " + path + " (" + count + ")");
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Deep Pocket", fixedRefs + " referencia/s reparada/s. Revisá la consola por los detalles.", "OK");
        }

        // Deduce a qué clase pertenecía el bloque según sus campos serializados (y el nombre del archivo).
        static string GuessType(string block, string fileName)
        {
            if (block.Contains("healthScalePerWave:")) return "EnemyDefinition";
            if (block.Contains("vfxPrefab:")) return "GroundSlamAbility";
            if (block.Contains("extraRange:")) return "ExtendAbility";
            if (block.Contains("damageMultiplier:") && block.Contains("maxTargets:"))
                return fileName.ToLowerInvariant().Contains("ring") ? "RingAbility" : "BurstAbility";
            if (block.Contains("gridConfig:") && block.Contains("lootTable:")) return "GameManager";
            return null;
        }

        static string ScriptGuid(string typeName)
        {
            if (guidCache.TryGetValue(typeName, out string cached)) return cached;
            string found = null;
            foreach (string guid in AssetDatabase.FindAssets("t:MonoScript " + typeName))
            {
                if (Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)) == typeName) { found = guid; break; }
            }
            guidCache[typeName] = found;
            return found;
        }
    }
}
#endif
