using System;
using System.Collections.Generic;
using System.Linq;

public class LootSystem
{
    private Dictionary<string, int> lootTable = new Dictionary<string, int>();

    public LootSystem(List<LootItem> lootItems)
    {
        lootItems.ForEach(item => lootTable[item.name] = item.dropChance);
    }

    public string GenerateLoot()
    {
        int totalChance = lootTable.Values.Sum();
        int randomValue = new Random().Next(0, totalChance);

        int cumulativeChance = 0;
        foreach (var item in lootTable)
        {
            cumulativeChance += item.Value;
            if (randomValue <= cumulativeChance)
            {
                return item.Key;
            }
        }

        return null; // No loot generated
    }
}
