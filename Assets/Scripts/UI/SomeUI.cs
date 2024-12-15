
using UnityEngine;
using GoPath.Navigation;

namespace GoPath.UI
{
    public class SomeUI : MonoBehaviour, IMovementBlocker
    {
        public bool IsBlocking()
        {
            return gameObject.activeInHierarchy;
        }
    }
}