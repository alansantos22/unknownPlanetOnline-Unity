using System;
using System.Collections.Generic;

public class Game
{
    private Player player;
    private Dungeon dungeon;
    private LootSystem lootSystem;
    private CombatSystem combatSystem;
    private TurnSystem turnSystem;
    private RewardSystem rewardSystem;

    public Game(string playerName)
    {
        player = new Player(playerName, 100f, 100f, 10f);
        dungeon = new Dungeon(player);
        lootSystem = new LootSystem(LootItems.lootItems);
        combatSystem = new CombatSystem(player, dungeon.GetMonsters());
        turnSystem = new TurnSystem(new List<Player> { player }, dungeon.GetMonsters());
        rewardSystem = new RewardSystem();
    }

    public void Start()
    {
        Console.WriteLine($"Welcome to the Dungeon, {player.Name}!");
        EnterDungeon();
    }

    private void EnterDungeon()
    {
        dungeon.SetupMonsters();
        Console.WriteLine("You have entered the dungeon!");
        StartCombat();
    }

    private void StartCombat()
    {
        while (dungeon.HasMonsters())
        {
            var currentMonster = dungeon.GetNextMonster();
            turnSystem.ExecuteTurn();
            
            if (player.Hp <= 0)
            {
                Console.WriteLine("You have been defeated!");
                return;
            }
            
            if (currentMonster.CanFlee && !dungeon.HasMonsters())
            {
                Console.WriteLine($"{currentMonster.Name} has fled the dungeon!");
                break;
            }
        }
        CollectRewards();
    }

    private void CollectRewards()
    {
        rewardSystem.CalculateRewards(dungeon.GetMonsterCount());
        Console.WriteLine($"You have completed the dungeon and received: {rewardSystem.GetExperience()} EXP and {rewardSystem.GetGold()} gold!");
        lootSystem.GenerateLoot();
    }
}

public static class Program
{
    public static void Main()
    {
        var game = new Game("Hero");
        game.Start();
    }
}
