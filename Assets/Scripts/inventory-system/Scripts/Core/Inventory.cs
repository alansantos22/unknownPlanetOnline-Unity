using System.Collections.Generic;
using UnityEngine;

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
        foreach (ItemSlot slot in itemSlots)
        {
            if (slot.IsPiled && slot.Item != null && slot.Item.ID == item.ID)
            {
                if (slot.Quantity + quantity <= 64)
                {
                    slot.Quantity += quantity;
                    return;
                }
            }
        }

        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].Item == null)
            {
                itemSlots[i].Item = item;
                itemSlots[i].Quantity = quantity;
                return;
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