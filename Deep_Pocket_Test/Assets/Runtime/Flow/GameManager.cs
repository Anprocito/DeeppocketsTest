using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Raíz de composición: crea los componentes que falten, los inicializa en orden y arma la UI.
    public class GameManager : MonoBehaviour
    {
        [Header("Datos (ScriptableObjects)")]
        public GridConfig gridConfig;
        public CharacterDefinition character;
        public WaveDefinition waves;
        public LootTable lootTable;
        public StatUpgradeDefinition[] statUpgrades;

        [Header("Escena")]
        public Transform player;

        [Header("Reglas")]
        [Tooltip("Segundos que hay que sobrevivir para ganar (300 = 5 minutos).")]
        [Min(1f)] public float victoryTimeSeconds = 300f;

        [Header("Debug")]
        [Tooltip("L = subir de nivel al instante. C = mostrar/ocultar abanicos de cobertura.")]
        public bool debugKeys = true;

        public int Kills { get; private set; }
        public float TimeAlive { get; private set; }

        PlayerInventory inventory;
        PlayerStats stats;
        PlayerHealth health;
        PlayerExperience experience;
        EnemySpawner spawner;
        GameUI ui;

        void Awake()
        {
            GameState.Reset();

            if (player == null)
            {
                PlayerHealth found = FindFirstObjectByType<PlayerHealth>();
                if (found != null) player = found.transform;
            }
            if (player == null)
            {
                Debug.LogError("GameManager: asigná el Player en el campo 'player'.", this);
                enabled = false;
                return;
            }

            if (gridConfig == null) gridConfig = ScriptableObject.CreateInstance<GridConfig>();
            if (character == null) character = ScriptableObject.CreateInstance<CharacterDefinition>();
            if (statUpgrades == null || statUpgrades.Length == 0) statUpgrades = DefaultUpgrades();

            GameObject p = player.gameObject;
            inventory = Require<PlayerInventory>(p);
            stats = Require<PlayerStats>(p);
            health = Require<PlayerHealth>(p);
            experience = Require<PlayerExperience>(p);
            WeaponManager weapons = Require<WeaponManager>(p);
            ConsumableRunner consumables = Require<ConsumableRunner>(p);
            Require<PlayerMovement>(p);

            inventory.Initialize(gridConfig);
            stats.Initialize(character, inventory);
            health.Initialize(stats);
            XpOrb.Configure(experience, stats);
            weapons.Initialize(inventory, stats, health);
            consumables.Initialize(inventory, health);

            spawner = Require<EnemySpawner>(gameObject);
            spawner.Initialize(waves, player, health);

            ui = GameUIBuilder.Build(this, inventory, weapons, health, experience, spawner, Restart);

            LevelUpFlow flow = Require<LevelUpFlow>(gameObject);
            flow.Initialize(experience, stats, inventory, lootTable, statUpgrades, ui.levelUpPanel, ui.offerPanel);

            health.Died += OnPlayerDied;
            Enemy.Killed += OnEnemyKilled;

            // Equipo inicial del personaje.
            if (character.startingItems != null)
                foreach (ItemDefinition def in character.startingItems)
                    if (def != null) inventory.AddAutoPlaced(def);
        }

        void OnDestroy()
        {
            Enemy.Killed -= OnEnemyKilled;
            Time.timeScale = 1f;
        }

        void Update()
        {
            if (GameState.IsPlaying)
            {
                TimeAlive += Time.deltaTime;
                if (!health.IsDead && TimeAlive >= victoryTimeSeconds)
                {
                    OnVictory();
                    return;
                }
            }

            if (debugKeys && Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame && GameState.IsPlaying)
                experience.AddXp(experience.XpToNext);
        }

        void OnEnemyKilled(Enemy e) => Kills++;

        void OnPlayerDied()
        {
            GameState.Set(GameStateType.GameOver);
            ui.gameOver.Show(Kills, TimeAlive, experience.Level, spawner.CurrentWave);
        }

        void OnVictory()
        {
            TimeAlive = victoryTimeSeconds;
            GameState.Set(GameStateType.Victory);
            ui.gameOver.ShowVictory(Kills, TimeAlive, experience.Level, spawner.CurrentWave);
        }

        public void Restart()
        {
            GameState.Reset();
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0) SceneManager.LoadScene(scene.buildIndex);
            else SceneManager.LoadScene(scene.name);
        }

        static T Require<T>(GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        static StatUpgradeDefinition[] DefaultUpgrades()
        {
            StatUpgradeDefinition Make(string name, string desc, StatType stat, float value)
            {
                StatUpgradeDefinition u = ScriptableObject.CreateInstance<StatUpgradeDefinition>();
                u.displayName = name;
                u.description = desc;
                u.modifier = new StatModifier { stat = stat, op = ModifierOp.PercentAdd, value = value };
                return u;
            }
            return new[]
            {
                Make("Vida", "+20% vida máxima.", StatType.MaxHealth, 0.20f),
                Make("Daño", "+15% daño.", StatType.Damage, 0.15f),
                Make("Velocidad", "+8% velocidad de movimiento.", StatType.MoveSpeed, 0.08f)
            };
        }
    }
}
