#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using DeepPocket.Data;
using DeepPocket.Runtime;

namespace DeepPocket.EditorTools
{
    // Menú: DeepPocket > Armar escena de prueba
    // Usalo en una escena VACÍA. Crea assets de ejemplo, el layer Enemy, piso, jugador, cámara y GameManager.
    public static class SceneBuilder
    {
        [MenuItem("DeepPocket/Armar escena de prueba")]
        public static void Build()
        {
            if (GameObject.Find("GameManager") != null &&
                !EditorUtility.DisplayDialog("Deep Pocket", "Ya hay un GameManager en la escena. ¿Armar de todos modos?", "Sí", "Cancelar"))
                return;

            SampleAssetsCreator.CreateAll();
            EnsureLayer("Enemy");

            // Luz (por si la escena está vacía)
            if (Object.FindFirstObjectByType<Light>() == null)
            {
                GameObject lightGo = new GameObject("Directional Light");
                Light l = lightGo.AddComponent<Light>();
                l.type = LightType.Directional;
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            // Piso provisorio: reemplazalo con tu arena de ProBuilder.
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Piso (reemplazar con ProBuilder)";
            ground.transform.localScale = new Vector3(20f, 1f, 20f);

            // Jugador
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            Rigidbody rb = player.AddComponent<Rigidbody>();
            rb.useGravity = false;
            VisualTint(player, new Color(0.93f, 0.89f, 0.82f));

            // Cámara
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            TopDownCamera follow = cam.GetComponent<TopDownCamera>();
            if (follow == null) follow = cam.gameObject.AddComponent<TopDownCamera>();
            follow.target = player.transform;

            // GameManager con todos los datos asignados
            GameObject gmGo = new GameObject("GameManager");
            GameManager gm = gmGo.AddComponent<GameManager>();
            gm.player = player.transform;
            gm.gridConfig = FindAsset<GridConfig>();
            gm.waves = FindAsset<WaveDefinition>();
            gm.lootTable = FindAsset<LootTable>();
            gm.character = FindByName<CharacterDefinition>("Errante");
            gm.statUpgrades = new[]
            {
                FindByName<StatUpgradeDefinition>("Vida"),
                FindByName<StatUpgradeDefinition>("Danio"),
                FindByName<StatUpgradeDefinition>("Velocidad")
            }.Where(u => u != null).ToArray();

            SaveSceneAndRegister();
            Selection.activeGameObject = gmGo;
            Debug.Log("DeepPocket: escena lista. Dale Play. Controles: WASD, C = abanicos, L = subir de nivel (debug).");
        }

        static void VisualTint(GameObject go, Color c)
        {
            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial.color = c;   // modifica el material por defecto del primitivo: aceptable en un prototipo
        }

        static T FindAsset<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            return guids.Length == 0 ? null : AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        static T FindByName<T>(string assetName) where T : Object
        {
            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == assetName) return AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return FindAsset<T>();
        }

        static void EnsureLayer(string layerName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            for (int i = 8; i < 32; i++)
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName) return;
            for (int i = 8; i < 32; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(slot.stringValue)) continue;
                slot.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return;
            }
            Debug.LogWarning("DeepPocket: no hay un layer libre para 'Enemy'. Funciona igual, pero consulta contra todos los layers.");
        }

        // El botón "Intentar de nuevo" recarga la escena, así que tiene que estar guardada y en Build Settings.
        static void SaveSceneAndRegister()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(scene.path))
            {
                Directory.CreateDirectory("Assets/DeepPocket/Scenes");
                EditorSceneManager.SaveScene(scene, "Assets/DeepPocket/Scenes/DeepPocketTest.unity");
            }
            else EditorSceneManager.SaveScene(scene);

            var list = EditorBuildSettings.scenes.ToList();
            if (!list.Any(s => s.path == scene.path))
            {
                list.Add(new EditorBuildSettingsScene(scene.path, true));
                EditorBuildSettings.scenes = list.ToArray();
            }
        }
    }
}
#endif
