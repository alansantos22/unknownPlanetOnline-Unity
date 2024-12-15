using UnityEngine;

[System.Serializable]
public class ItemSlot
{
    public Item Item { get; set; }
    public int Quantity { get; set; }
    public bool IsPiled => Item != null && Item.IsPiled;

    public ItemSlot()
    {
        Item = null;
        Quantity = 0;
    }

    public void Clear()
    {
        Item = null;
        Quantity = 0;
    }
}