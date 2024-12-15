using System.Collections.Generic;

public static class Monsters
{
    public static List<MonsterData> monsters = new List<MonsterData>
    {
        new MonsterData
        {
            name = "Goblin",
            hp = 30,
            stamina = 10,
            agility = 5,
            attack = 8,
            defense = 2,
            minCount = 1,
            maxCount = 3,
        },
        new MonsterData
        {
            name = "Orc",
            hp = 50,
            stamina = 15,
            agility = 4,
            attack = 12,
            defense = 5,
            minCount = 2,
            maxCount = 4,
        },
        new MonsterData
        {
            name = "Troll",
            hp = 80,
            stamina = 20,
            agility = 3,
            attack = 15,
            defense = 8,
            minCount = 1,
            maxCount = 2,
        },
        new MonsterData
        {
            name = "Dragon",
            hp = 150,
            stamina = 30,
            agility = 2,
            attack = 25,
            defense = 10,
            minCount = 1,
            maxCount = 1,
        },
    };
}

public class MonsterData
{
    public string name;
    public int hp;
    public int stamina;
    public int agility;
    public int attack;
    public int defense;
    public int minCount;
    public int maxCount;
}
