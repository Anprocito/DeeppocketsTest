#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using DeepPocket.Data;

namespace DeepPocket.EditorTools
{
    // Menú: DeepPocket > Crear assets de ejemplo
    // Crea los assets con los valores del prototipo web. Si un asset ya existe NO se toca,
    // así que podés volver a ejecutarlo sin perder tus cambios de balance.
    public static class SampleAssetsCreator
    {
        const string Root = "Assets/DeepPocket/Data/Samples";

        [MenuItem("DeepPocket/Crear assets de ejemplo")]
        public static void CreateAll()
        {
            // ----- Config -----
            var grid = Make<GridConfig>("Config", "GridConfig", g => { });

            // ----- Habilidades -----
            var burst = Make<BurstAbility>("Abilities", "Burst", a =>
            {
                a.displayName = "Tajo amplio";
                a.description = "Golpea todo lo que esté en la zona del arma con daño extra.";
            });
            var extend = Make<ExtendAbility>("Abilities", "Extend", a =>
            {
                a.displayName = "Embestida";
                a.description = "Extiende el alcance del arma por un instante.";
            });
            var ring = Make<RingAbility>("Abilities", "Ring", a =>
            {
                a.displayName = "Torbellino";
                a.description = "Golpea los 360° en la banda de distancia del arma.";
            });
            Make<GroundSlamAbility>("Abilities", "GroundSlam", a =>
            {
                a.displayName = "Golpe sísmico";
                a.description = "Daña todo alrededor del jugador, haya o no enemigos.";
            });

            // ----- Armas (x = columna, y = fila) -----
            var daga = Make<WeaponDefinition>("Items", "Daga", w =>
            {
                Basic(w, "Daga", "Rápida y compacta.", "#9fd8cb"); Cells(w, V(0, 0));
                w.damage = 6f; w.attackInterval = 0.52f; w.maxTargetsPerAttack = 1;
                w.special = burst;
                w.specialTrigger = new AbilityTrigger { type = TriggerType.EveryNHits, everyN = 5 };
            });
            var espada = Make<WeaponDefinition>("Items", "Espada", w =>
            {
                Basic(w, "Espada", "Cubre dos bandas de alcance.", "#8fb6e8"); Cells(w, V(0, 0), V(0, 1));
                w.damage = 10f; w.attackInterval = 0.68f; w.maxTargetsPerAttack = 2;
                w.special = burst;
                w.specialTrigger = new AbilityTrigger { type = TriggerType.Cooldown, cooldownSeconds = 5f };
            });
            var lanza = Make<WeaponDefinition>("Items", "Lanza", w =>
            {
                Basic(w, "Lanza", "Mucho alcance: ocupa tres filas.", "#c9a6e8"); Cells(w, V(0, 0), V(0, 1), V(0, 2));
                w.damage = 9f; w.attackInterval = 0.78f; w.maxTargetsPerAttack = 3;
                w.special = extend;
                w.specialTrigger = new AbilityTrigger { type = TriggerType.Cooldown, cooldownSeconds = 7f };
            });
            var hacha = Make<WeaponDefinition>("Items", "Hacha", w =>
            {
                Basic(w, "Hacha", "Ancha: cubre dos rumbos a la vez.", "#e8a65c"); Cells(w, V(0, 0), V(1, 0));
                w.damage = 8f; w.attackInterval = 0.76f; w.maxTargetsPerAttack = 6;
                w.special = ring;
                w.specialTrigger = new AbilityTrigger { type = TriggerType.Cooldown, cooldownSeconds = 6f };
            });
            var vara = Make<WeaponDefinition>("Items", "Vara", w =>
            {
                Basic(w, "Vara", "Compacta y a distancia.", "#c9974f"); Cells(w, V(0, 0));
                w.damage = 5f; w.attackInterval = 0.46f; w.maxTargetsPerAttack = 1;
                w.isRanged = true; w.projectileSpeed = 14f;
                w.special = extend;
                w.specialTrigger = new AbilityTrigger { type = TriggerType.Cooldown, cooldownSeconds = 4.2f };
            });

            // ----- Pasivos -----
            var botas = Make<PassiveDefinition>("Items", "Botas", p =>
            {
                Basic(p, "Botas ligeras", "+16% velocidad de movimiento.", "#7fae6f"); Cells(p, V(0, 0));
                p.modifiers = new[] { Mod(StatType.MoveSpeed, ModifierOp.PercentAdd, 0.16f) };
            });
            var amuleto = Make<PassiveDefinition>("Items", "Amuleto", p =>
            {
                Basic(p, "Amuleto vital", "+22% vida máxima.", "#d989a8"); Cells(p, V(0, 0));
                p.modifiers = new[] { Mod(StatType.MaxHealth, ModifierOp.PercentAdd, 0.22f) };
            });

            // ----- Desplegable y consumible -----
            var mina = Make<DeployableDefinition>("Items", "Mina", d =>
            {
                Basic(d, "Mina", "Arrastrala fuera de la grilla para colocarla.", "#c96b4f"); Cells(d, V(0, 0));
                d.deployableKind = DeployableKind.Mine; d.radius = 3f; d.damage = 46f; d.duration = 0f;
            });
            var pocion = Make<ConsumableDefinition>("Items", "Pocion", c =>
            {
                Basic(c, "Poción", "Se usa sola con poca vida y libera su lugar.", "#c94f4f"); Cells(c, V(0, 0));
                c.triggerHealthFraction = 0.35f; c.healAmount = 34f;
            });

            // ----- Mejoras de estadística -----
            Make<StatUpgradeDefinition>("StatUpgrades", "Vida", s =>
            {
                s.displayName = "Vida"; s.description = "+20% vida máxima.";
                s.modifier = Mod(StatType.MaxHealth, ModifierOp.PercentAdd, 0.20f);
            });
            Make<StatUpgradeDefinition>("StatUpgrades", "Danio", s =>
            {
                s.displayName = "Daño"; s.description = "+15% daño.";
                s.modifier = Mod(StatType.Damage, ModifierOp.PercentAdd, 0.15f);
            });
            Make<StatUpgradeDefinition>("StatUpgrades", "Velocidad", s =>
            {
                s.displayName = "Velocidad"; s.description = "+8% velocidad de movimiento.";
                s.modifier = Mod(StatType.MoveSpeed, ModifierOp.PercentAdd, 0.08f);
            });

            // ----- Tabla de loot (ofertas de item al subir de nivel) -----
            Make<LootTable>("Config", "LootTable", t =>
            {
                t.entries = new[]
                {
                    E(daga, 3f), E(espada, 3f), E(lanza, 2f), E(hacha, 2f), E(vara, 3f),
                    E(botas, 2f), E(amuleto, 2f), E(mina, 2f), E(pocion, 3f)
                };
            });

            // ----- Personajes -----
            Make<CharacterDefinition>("Characters", "Errante", c =>
            {
                c.displayName = "Errante"; c.description = "+18% velocidad de movimiento.";
                c.startingModifiers = new[] { Mod(StatType.MoveSpeed, ModifierOp.PercentAdd, 0.18f) };
                c.startingItems = new ItemDefinition[] { daga };
            });
            Make<CharacterDefinition>("Characters", "Coleccionista", c =>
            {
                c.displayName = "Coleccionista"; c.description = "+35% radio de recolección y +15% oro.";
                c.goldMultiplier = 1.15f;
                c.startingModifiers = new[] { Mod(StatType.PickupRadius, ModifierOp.PercentAdd, 0.35f) };
                c.startingItems = new ItemDefinition[] { daga };
            });
            Make<CharacterDefinition>("Characters", "Fornido", c =>
            {
                c.displayName = "Fornido"; c.description = "+30% vida máxima, -10% velocidad.";
                c.startingModifiers = new[]
                {
                    Mod(StatType.MaxHealth, ModifierOp.PercentAdd, 0.30f),
                    Mod(StatType.MoveSpeed, ModifierOp.PercentAdd, -0.10f)
                };
                c.startingItems = new ItemDefinition[] { daga };
            });

            // ----- Enemigos y oleadas -----
            var normal = Make<EnemyDefinition>("Enemies", "Normal", e =>
            {
                e.displayName = "Normal"; e.color = new Color(0.56f, 0.35f, 0.35f); e.visualScale = 1f; e.maxHealth = 26f; e.moveSpeed = 2f; e.contactDamage = 9f;
                e.contactRange = 1.1f; e.xpReward = 8; e.goldReward = 2;
            });
            var rapido = Make<EnemyDefinition>("Enemies", "Rapido", e =>
            {
                e.displayName = "Rápido"; e.color = new Color(0.35f, 0.56f, 0.48f); e.visualScale = 0.8f; e.maxHealth = 13f; e.moveSpeed = 3.5f; e.contactDamage = 7f;
                e.contactRange = 1.0f; e.xpReward = 7; e.goldReward = 2;
            });
            var tanque = Make<EnemyDefinition>("Enemies", "Tanque", e =>
            {
                e.displayName = "Tanque"; e.color = new Color(0.35f, 0.35f, 0.56f); e.visualScale = 1.4f; e.maxHealth = 68f; e.moveSpeed = 1.2f; e.contactDamage = 14f;
                e.contactRange = 1.4f; e.xpReward = 16; e.goldReward = 4;
            });
            Make<WaveDefinition>("Enemies", "WaveDefinition", w =>
            {
                w.spawnTable = new[]
                {
                    new WaveDefinition.SpawnEntry { enemy = normal, firstWave = 1, weight = 58f },
                    new WaveDefinition.SpawnEntry { enemy = rapido, firstWave = 2, weight = 26f },
                    new WaveDefinition.SpawnEntry { enemy = tanque, firstWave = 4, weight = 16f }
                };
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = grid;
            EditorGUIUtility.PingObject(grid);
            Debug.Log("DeepPocket: assets de ejemplo listos en " + Root);
        }

        // ---------- helpers ----------
        static T Make<T>(string sub, string assetName, Action<T> configure) where T : ScriptableObject
        {
            string folder = Root + "/" + sub;
            EnsureFolder(folder);
            string path = folder + "/" + assetName + ".asset";

            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;

            T instance = ScriptableObject.CreateInstance<T>();
            configure?.Invoke(instance);
            AssetDatabase.CreateAsset(instance, path);
            return instance;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        static Vector2Int V(int x, int y) => new Vector2Int(x, y);
        static void Cells(ItemDefinition d, params Vector2Int[] cells) => d.SetCells(cells);

        static void Basic(ItemDefinition d, string displayName, string description, string hex)
        {
            d.displayName = displayName;
            d.description = description;
            if (ColorUtility.TryParseHtmlString(hex, out Color c)) d.color = c;
        }

        static StatModifier Mod(StatType stat, ModifierOp op, float value)
            => new StatModifier { stat = stat, op = op, value = value };

        static LootTable.Entry E(ItemDefinition item, float weight)
            => new LootTable.Entry { item = item, weight = weight };
    }
}
#endif
