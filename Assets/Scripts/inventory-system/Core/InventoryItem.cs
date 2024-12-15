using UnityEngine;
using InventorySystem.Items;

namespace InventorySystem.Core
{
    [System.Serializable]
    public class InventoryItem
    {
        public ItemData data;
        public int stackSize;

        public InventoryItem(ItemData data, int amount = 1)
        {
            this.data = data;
            this.stackSize = amount;
        }
    }
}
