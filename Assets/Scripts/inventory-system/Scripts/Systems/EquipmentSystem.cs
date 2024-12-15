using UnityEngine;
using System.Collections.Generic;

public class EquipmentSystem : MonoBehaviour
{
    public Dictionary<ItemType, Item> equippedItems = new Dictionary<ItemType, Item>();
    public Item offHandItem;
    private PlayerStats playerStats;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    public void EquipItem(Item item, PlayerStats playerStats)
    {
        if (!item.IsEquippable) return;

        if (equippedItems.ContainsKey(item.Type))
        {
            UnequipItem(item.Type, playerStats);
        }

        equippedItems[item.Type] = item;
        playerStats.AddStats(item);
    }

    public void UnequipItem(ItemType type, PlayerStats playerStats)
    {
        if (equippedItems.ContainsKey(type))
        {
            playerStats.RemoveStats(equippedItems[type]);
            equippedItems.Remove(type);
        }
    }
}