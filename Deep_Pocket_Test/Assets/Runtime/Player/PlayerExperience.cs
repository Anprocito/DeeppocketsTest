using System;
using UnityEngine;

namespace DeepPocket.Runtime
{
    public class PlayerExperience : MonoBehaviour
    {
        public int Level { get; private set; } = 1;
        public float Xp { get; private set; }
        public int XpToNext => 24 + (Level - 1) * 15;

        public event Action<int> LeveledUp;   // nuevo nivel (se dispara una vez por cada nivel ganado)
        public event Action Changed;

        public void AddXp(int amount)
        {
            Xp += amount;
            while (Xp >= XpToNext)
            {
                int need = XpToNext;
                Xp -= need;
                Level++;
                LeveledUp?.Invoke(Level);
            }
            Changed?.Invoke();
        }
    }
}
