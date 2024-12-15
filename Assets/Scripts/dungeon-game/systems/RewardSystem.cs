using System;

public class RewardSystem
{
    private int experience;
    private int gold;

    public RewardSystem()
    {
        experience = 0;
        gold = 0;
    }

    public void CalculateRewards(int monsterCount)
    {
        experience += monsterCount * 10; // Example: 10 EXP per monster
        gold += GenerateGold(monsterCount);
    }

    private int GenerateGold(int monsterCount)
    {
        const int baseGold = 5; // Base gold per monster
        var randomBonus = new Random().Next(0, 5); // Random bonus between 0 and 4
        return monsterCount * (baseGold + randomBonus);
    }

    public int GetExperience()
    {
        return experience;
    }

    public int GetGold()
    {
        return gold;
    }

    public void ResetRewards()
    {
        experience = 0;
        gold = 0;
    }
}
