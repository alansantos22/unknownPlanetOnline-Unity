public static class Calculator
{
    public static float CalculateAttackPower(float atk, float strength, float hp)
    {
        return atk * (strength * 0.15f) + hp / 5;
    }

    public static float CalculateMonsterStrength(Monster monster)
    {
        // Calculate strength based on existing monster properties
        float derivedStrength = (monster.AttackPower + monster.Defense) / 2f;
        return CalculateAttackPower(monster.AttackPower, derivedStrength, monster.Hp);
    }

    public static int CalculateMonsterDamage(Monster monster)
    {
        return (int)(monster.AttackPower * (1 + monster.Agility / 100));
    }
}
