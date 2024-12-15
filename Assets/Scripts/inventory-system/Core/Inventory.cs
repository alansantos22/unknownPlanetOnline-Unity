using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Items; // Adicione esta linha

public class Inventory : MonoBehaviour
{
    public List<ItemSlot> itemSlots;
    public int maxSlots = 20;

    private void Start()
    {
        itemSlots = new List<ItemSlot>(maxSlots);
        for (int i = 0; i < maxSlots; i++)
        {
            itemSlots.Add(new ItemSlot());
        }
    }

    public void AddItem(Item item, int quantity)
    {
        // Se o item pode ser empilhado, tenta adicionar à pilha existente
        if (item.IsPiled)
        {
            foreach (ItemSlot slot in itemSlots)
            {
                if (slot.IsPiled && slot.Item != null && slot.Item.ID == item.ID)
                {
                    if (slot.Quantity + quantity <= item.MaxStack)
                    {
                        slot.Quantity += quantity;
                        return;
                    }
                }
            }
        }

        // Se não pode ser empilhado ou não encontrou pilha existente,
        // procura um slot vazio para cada unidade
        int remainingQuantity = quantity;
        while (remainingQuantity > 0)
        {
            ItemSlot emptySlot = itemSlots.Find(slot => slot.Item == null);
            if (emptySlot != null)
            {
                emptySlot.Item = item;
                emptySlot.Quantity = item.IsPiled ? remainingQuantity : 1;
                remainingQuantity = item.IsPiled ? 0 : remainingQuantity - 1;
            }
            else
            {
                Debug.LogWarning("Inventory is full!");
                break;
            }
        }
    }

    public void RemoveItem(Item item, int quantity)
    {
        foreach (ItemSlot slot in itemSlots)
        {
            if (slot.Item != null && slot.Item.ID == item.ID)
            {
                if (slot.Quantity >= quantity)
                {
                    slot.Quantity -= quantity;
                    if (slot.Quantity <= 0)
                    {
                        slot.Item = null;
                    }
                    return;
                }
            }
        }
    }
}