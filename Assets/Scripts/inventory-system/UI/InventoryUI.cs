using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform itemSlotContainer;
    [SerializeField] private ItemSlotUI itemSlotPrefab;
    [SerializeField] private InventoryGridLayout gridLayout;
    [SerializeField] private PlayerStats playerStats; // Referência ao PlayerStats
    
    private Inventory inventory;
    private List<ItemSlotUI> itemSlotUIs = new List<ItemSlotUI>();

    private void Awake()
    {
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError("PlayerStats not found in scene.");
                return;
            }
        }

        inventory = playerStats.GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Inventory component not found on player.");
            return;
        }

        if (gridLayout == null)
        {
            gridLayout = itemSlotContainer.GetComponent<InventoryGridLayout>();
            if (gridLayout == null)
            {
                gridLayout = itemSlotContainer.gameObject.AddComponent<InventoryGridLayout>();
            }
        }
    }

    private void Start()
    {
        if (inventory != null)
        {
            InitializeInventoryUI();
            UpdateGridLayout();
        }
    }

    private void InitializeInventoryUI()
    {
        if (inventory != null)
        {
            for (int i = 0; i < inventory.maxSlots; i++)
            {
                ItemSlotUI slot = Instantiate(itemSlotPrefab, itemSlotContainer);
                itemSlotUIs.Add(slot);
                slot.SetItemSlot(inventory.itemSlots[i]);
            }
        }
    }

    private void UpdateGridLayout()
    {
        if (inventory != null)
        {
            RectTransform containerRect = itemSlotContainer as RectTransform;
            if (containerRect != null)
            {
                float containerWidth = containerRect.rect.width;
                float containerHeight = containerRect.rect.height;
                
                // Calcule quantas colunas cabem na largura do container
                float slotSizeWithSpacing = 90f; // 80 (slot) + 10 (spacing)
                int maxColumns = Mathf.FloorToInt(containerWidth / slotSizeWithSpacing);
                
                // Ajuste as colunas para um valor razoável (entre 4 e 8)
                int columns = Mathf.Clamp(maxColumns, 4, 8);
                
                gridLayout.UpdateColumns(columns);
            }
        }
    }

    public void UpdateUI()
    {
        if (inventory != null)
        {
            for (int i = 0; i < itemSlotUIs.Count; i++)
            {
                itemSlotUIs[i].UpdateUI();
            }
        }
    }
}