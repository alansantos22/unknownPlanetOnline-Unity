using UnityEngine;
using GoPath.Navigation;

namespace GoPath.UI
{
    public class ConstructionUI : MonoBehaviour
    {
        public bool IsActive => gameObject.activeInHierarchy;

        private void OnEnable()
        {
            MovementBlocker.AddBlock();
        }

        private void OnDisable()
        {
            MovementBlocker.RemoveBlock();
        }
    }
}
