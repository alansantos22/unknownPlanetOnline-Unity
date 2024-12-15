using System;

public static class RandomGenerator
{
    private static readonly Random random = new Random();

    public static int GetRandomInt(int min, int max)
    {
        return random.Next(min, max + 1);
    }

    public static float GetRandomFloat(float min, float max)
    {
        return (float)(random.NextDouble() * (max - min) + min);
    }

    public static bool GetRandomChance(double chance)
    {
        return random.NextDouble() < chance;
    }
}
