using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace UnknownPlanet
{
    public class ConstructionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_Dropdown buildingTypeDropdown;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private Button cancelButton;  // Add this line

        private ConstructionManager constructionManager;
        private Vector2Int coordinates;

        void Awake()
        {
            // Setup building type dropdown
            buildingTypeDropdown.ClearOptions();
            buildingTypeDropdown.AddOptions(System.Enum.GetNames(typeof(BuildingType)).ToList());
            
            confirmButton.onClick.AddListener(OnConfirmClicked);
            cancelButton.onClick.AddListener(OnCancelClicked);  // Add this line
            
            if (errorText != null)
                errorText.gameObject.SetActive(false);

            gameObject.SetActive(false);
        }

        public void Initialize(ConstructionManager manager, Vector2Int coords)
        {
            constructionManager = manager;
            coordinates = coords;
            
            // Reset UI state
            nameInput.text = "";
            buildingTypeDropdown.value = 0;
            if (errorText != null)
                errorText.gameObject.SetActive(false);
            
            gameObject.SetActive(true);
        }

        private void OnConfirmClicked()
        {
            if (string.IsNullOrEmpty(nameInput.text))
            {
                ShowError("Construction name cannot be empty");
                return;
            }

            var type = (BuildingType)buildingTypeDropdown.value;
            if (!constructionManager.CanBuildAt(type, coordinates))
            {
                ShowError($"Cannot build {type} here - too close to another {type}");
                return;
            }

            constructionManager.CreateConstruction(nameInput.text, type, coordinates);
            constructionManager.CloseConstructionUI();  // Usar o novo método
        }

        private void OnCancelClicked()
        {
            if (constructionManager != null)
            {
                constructionManager.CloseConstructionUI();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning(message);
            }
        }
    }
}