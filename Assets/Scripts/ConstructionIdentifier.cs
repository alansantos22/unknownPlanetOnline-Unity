using UnityEngine;
using UnityEngine.EventSystems;

namespace UnknownPlanet
{
    public class ConstructionIdentifier : MonoBehaviour
    {
        public Construction constructionData { get; private set; }

        public void Initialize(Construction data)
        {
            constructionData = data;
        }

        void OnMouseDown()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log($"Clicked on {constructionData.name} ({constructionData.type})");
                // Aqui você pode adicionar lógica para mostrar informações da construção
            }
        }
    }
}