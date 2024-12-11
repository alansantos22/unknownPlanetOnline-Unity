using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace UnknownPlanet
{
    public class ConstructionIdentifier : MonoBehaviour
    {
        [SerializeField] private TextMeshPro labelText;
        public Construction constructionData { get; private set; }

        public void Initialize(Construction data)
        {
            constructionData = data;
            if (labelText == null)
                labelText = GetComponentInChildren<TextMeshPro>();
                
            if (labelText != null)
                labelText.text = data.name;
        }

        public void SetLabelVisible(bool visible)
        {
            if (labelText != null)
                labelText.enabled = visible;
        }

        void OnMouseDown()
        {
            if (!EventSystem.current.IsPointerOverGameObject() && constructionData != null)
            {
                Debug.Log($"Clicked on {constructionData.name} ({constructionData.type})");
                // Aqui você pode adicionar lógica para mostrar informações da construção
            }
        }
    }
}