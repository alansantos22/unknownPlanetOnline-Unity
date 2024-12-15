using System;
using System.Collections.Generic;
using System.Linq;

public class Dungeon
{
    private Player player;
    private List<Monster> monsters;
    private int currentMonsterIndex;
    private LootSystem lootSystem;

    public Dungeon(Player player)
    {
        this.player = player;
        this.monsters = new List<Monster>();
        this.currentMonsterIndex = 0;
        this.lootSystem = new LootSystem(LootItems.lootItems);
    }

    public void EnterDungeon()
    {
        SetupMonsters();
        StartCombat();
    }

    public void ManageMonsters()
    {
        this.monsters = this.monsters.Where(monster => monster.Hp > 0).ToList();
    }

    public int GetMonsterCount()
    {
        return this.monsters.Count;
    }

    public void SetupMonsters()
    {
        monsters.Add(new Monster("Goblin", 50f, 30f, 5f, 5, 2));
        monsters.Add(new Monster("Orc", 80f, 40f, 3f, 8, 4));
        monsters.Add(new Monster("Troll", 120f, 60f, 2f, 12, 6));
    }

    public bool HasMonsters()
    {
        return currentMonsterIndex < monsters.Count;
    }

    public Monster GetNextMonster()
    {
        if (HasMonsters())
            return monsters[currentMonsterIndex++];
        return null;
    }

    public List<Monster> GetMonsters() => monsters;

    public void LootRewards()
    {
        string loot = this.lootSystem.GenerateLoot();
        if (loot != null)
        {
            Console.WriteLine($"Player found: {loot}");
        }
    }

    private void StartCombat()
    {
        while (this.player.Hp > 0 && this.monsters.Count > 0)
        {
            // Combat round logic here
            ManageMonsters();
        }
    }
}
