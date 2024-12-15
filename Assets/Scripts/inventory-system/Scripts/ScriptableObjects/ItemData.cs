
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public int id;
    public string itemName;
    public Sprite icon;
    public ItemRarity rarity;
    public bool isStackable;
    public int maxStackSize = 99;
    [TextArea(3, 10)]
    public string description;
}