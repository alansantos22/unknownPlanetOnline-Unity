using System;

public static class BattleSystem
{
    public static double CalculateDefense()
    {
        return new Random().NextDouble() * (0.3 - 0.2) + 0.2;
    }

    public static int CalculatePlayerAttack(int attackPower, int targetDefense)
    {
        double defenseMultiplier = CalculateDefense();
        int damage = attackPower - (int)(targetDefense * defenseMultiplier);
        return damage < 0 ? 1 : damage;
    }

    public static int CalculateMonsterAttack(Monster monster, int targetDefense)
    {
        return (int)(monster.AttackPower * (1 - targetDefense / 100f));
    }

    public static int CalculateCriticalHit(int damage, double chance = 0.1, int multiplier = 2)
    {
        if (new Random().NextDouble() <= chance)
        {
            return damage * multiplier;
        }
        return damage;
    }
}
