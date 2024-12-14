using UnityEngine;

namespace GoPath.Navigation
{
    public interface IMovementBlocker
    {
        bool IsBlocking();
    }

    public class SomeUI : MonoBehaviour, IMovementBlocker
    {
        public bool IsBlocking()
        {
            return gameObject.activeInHierarchy; // ou qualquer outra condição
        }
    }
}