using System;
using UnityEngine;

namespace GoPath.Navigation
{
    public static class MovementBlocker
    {
        public static event Action<bool> OnMovementBlockChanged;
        private static int blockCount = 0;

        public static bool IsBlocked => blockCount > 0;

        public static void AddBlock()
        {
            blockCount++;
            if (blockCount == 1) // Primeira vez que está sendo bloqueado
                OnMovementBlockChanged?.Invoke(true);
        }

        public static void RemoveBlock()
        {
            blockCount = Mathf.Max(0, blockCount - 1);
            if (blockCount == 0) // Último bloqueio removido
                OnMovementBlockChanged?.Invoke(false);
        }
    }
}
