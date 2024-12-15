using System.Collections.Generic;

public static class Dungeons
{
    public static List<DungeonData> dungeons = new List<DungeonData>
    {
        new DungeonData
        {
            name = "Goblin Cave",
            description = "A dark cave filled with mischievous goblins.",
            minMonsters = 3,
            maxMonsters = 5,
            monsterList = new List<string> { "Goblin", "Goblin Archer" },
            loot = new Loot { gold = new Range { min = 10, max = 30 }, items = new List<string> { "Health Potion", "Goblin Sword" } }
        },
        new DungeonData
        {
            name = "Skeleton Crypt",
            description = "An ancient crypt haunted by skeletons.",
            minMonsters = 2,
            maxMonsters = 4,
            monsterList = new List<string> { "Skeleton Warrior", "Skeleton Mage" },
            loot = new Loot { gold = new Range { min = 20, max = 50 }, items = new List<string> { "Bone Shield", "Mana Potion" } }
        },
        new DungeonData
        {
            name = "Dragon's Lair",
            description = "A treacherous lair guarded by a fierce dragon.",
            minMonsters = 1,
            maxMonsters = 2,
            monsterList = new List<string> { "Dragon" },
            loot = new Loot { gold = new Range { min = 100, max = 200 }, items = new List<string> { "Dragon Scale", "Treasure Chest" } }
        }
    };
}

public class DungeonData
{
    public string name;
    public string description;
    public int minMonsters;
    public int maxMonsters;
    public List<string> monsterList;
    public Loot loot;
}

public class Loot
{
    public Range gold;
    public List<string> items;
}

public class Range
{
    public int min;
    public int max;
}
