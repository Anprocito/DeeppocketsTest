using System;
using UnityEngine;

namespace DeepPocket.Runtime
{
    public enum GameStateType { Playing, LevelUp, GameOver, Victory }

    // Fuente única de verdad sobre si el juego corre. Además de avisar por evento, congela Time.timeScale
    // (así enemigos, proyectiles y física se frenan solos) mientras la UI sigue funcionando.
    public static class GameState
    {
        public static GameStateType Current { get; private set; } = GameStateType.Playing;
        public static bool IsPlaying => Current == GameStateType.Playing;
        public static event Action<GameStateType> Changed;

        public static void Set(GameStateType next)
        {
            if (Current == next) return;
            Current = next;
            Time.timeScale = next == GameStateType.Playing ? 1f : 0f;
            Changed?.Invoke(next);
        }

        public static void Reset()
        {
            Current = GameStateType.Playing;
            Time.timeScale = 1f;
            Changed = null;
        }

        // Necesario si tenés desactivado el "Domain Reload" al entrar a Play Mode.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnLoad() => Reset();
    }
}
