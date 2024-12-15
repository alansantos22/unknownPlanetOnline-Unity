using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform itemSlotContainer;
    [SerializeField] private ItemSlotUI itemSlotPrefab;
    
    private Inventory inventory;
    private List<ItemSlotUI> itemSlotUIs = new List<ItemSlotUI>();

    private void Start()
    {
        inventory = GetComponent<Inventory>();
        InitializeInventoryUI();
    }

    private void InitializeInventoryUI()
    {
        for (int i = 0; i < inventory.maxSlots; i++)
        {
            ItemSlotUI slot = Instantiate(itemSlotPrefab, itemSlotContainer);
            itemSlotUIs.Add(slot);
            slot.SetItemSlot(inventory.itemSlots[i]);
        }
    }

    public void UpdateUI()
    {
        for (int i = 0; i < itemSlotUIs.Count; i++)
        {
            itemSlotUIs[i].UpdateUI();
        }
    }
}