using System;

public class Player : ICharacter
{
    public string Name { get; set; }
    public float Hp { get; set; }
    public float Stamina { get; set; }
    public float Agility { get; set; }
    public int AttackPower { get; private set; }
    public int Defense { get; private set; }

    public Player(string name, float hp, float stamina, float agility)
    {
        Name = name;
        Hp = hp;
        Stamina = stamina;
        Agility = agility;
        AttackPower = 10;
        Defense = 5;
    }

    public void Attack(ICharacter target)
    {
        int damage = BattleSystem.CalculatePlayerAttack(this.AttackPower, this.Defense);
        target.Hp -= damage;
    }

    public void Defend()
    {
        this.Defense = (int)(this.Defense * 1.5);
        this.Stamina -= 2;
    }

    public void Flee()
    {
        if (this.Stamina >= 5)
        {
            this.Stamina -= 5;
            // Implementation will depend on dungeon mechanics
        }
    }

    public void AddBuff(string buffType, float duration)
    {
        switch (buffType.ToLower())
        {
            case "fear":
                AttackPower = (int)(AttackPower * 0.8f); // Reduz o ataque em 20%
                break;
            case "slow":
                Agility *= 0.7f; // Reduz a agilidade em 30%
                break;
            case "weakness":
                Defense = (int)(Defense * 0.8f); // Reduz a defesa em 20%
                break;
        }
        
        Console.WriteLine($"{Name} is affected by {buffType} for {duration} turns!");
    }
}
