using System.Collections.Generic;

public interface ILoot
{
    string name { get; set; }
    int dropChance { get; set; }
}

public class LootItem : ILoot
{
    public string name { get; set; }
    public int dropChance { get; set; }
    public string Name { get; set; }
    public float DropChance { get; set; }
}

public static class LootItems
{
    public static List<LootItem> lootItems = new List<LootItem>
    {
        new LootItem { name = "Health Potion", dropChance = 30 },
        new LootItem { name = "Mana Potion", dropChance = 20 },
        new LootItem { name = "Gold Coin", dropChance = 50 },
        new LootItem { name = "Sword of Strength", dropChance = 10 },
        new LootItem { name = "Shield of Resilience", dropChance = 15 },
        new LootItem { name = "Mystic Amulet", dropChance = 5 }
    };
}
