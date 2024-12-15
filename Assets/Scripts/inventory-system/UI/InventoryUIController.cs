using UnityEngine;
using GoPath.Navigation;

public class InventoryUIController : MonoBehaviour 
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject equipmentPanel;
    
    [SerializeField] private KeyCode inventoryKey = KeyCode.I;
    [SerializeField] private KeyCode equipmentKey = KeyCode.E;
    
    private bool isInventoryOpen;
    private bool isEquipmentOpen;

    private void Start()
    {
        SetPanelsActive(false, false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventory();
        }
        
        if (Input.GetKeyDown(equipmentKey))
        {
            ToggleEquipment();
        }
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        UpdatePanelsState();
    }

    private void ToggleEquipment()
    {
        isEquipmentOpen = !isEquipmentOpen;
        UpdatePanelsState();
    }

    private void UpdatePanelsState()
    {
        SetPanelsActive(isInventoryOpen, isEquipmentOpen);
    }

    private void SetPanelsActive(bool inventoryActive, bool equipmentActive)
    {
        inventoryPanel.SetActive(inventoryActive);
        equipmentPanel.SetActive(equipmentActive);
        
        if (inventoryActive || equipmentActive)
            MovementBlocker.AddBlock();
        else
            MovementBlocker.RemoveBlock();
    }
}
