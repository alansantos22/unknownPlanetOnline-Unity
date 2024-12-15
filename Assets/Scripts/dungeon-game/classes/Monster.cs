using System;
using System.Collections.Generic;
using System.Linq;

public class Monster : ICharacter
{
    // Base properties
    public string Name { get; set; }
    public float Hp { get; set; }
    public float Stamina { get; set; }
    public float Agility { get; set; }
    public int AttackPower { get; private set; }
    public int Defense { get; private set; }

    // Additional properties
    public float MaxHp { get; private set; }
    public float MaxStamina { get; private set; }
    public float Mana { get; set; }
    public float MaxMana { get; private set; }
    public bool IsMage { get; private set; }
    public bool CanFlee { get; private set; }

    // AI related fields
    private readonly Random random = new Random();
    private const float STAMINA_COST_PER_ATTACK = 5f;
    private const float FLEE_THRESHOLD = 0.3f;
    private const float REST_THRESHOLD = 0.2f;

    public Monster(string name, float hp, float stamina, float agility, int attackPower, 
                  int defense, bool isMage = false, bool canFlee = false)
    {
        Name = name;
        MaxHp = Hp = hp;
        MaxStamina = Stamina = stamina;
        Agility = agility;
        AttackPower = attackPower;
        Defense = defense;
        IsMage = isMage;
        CanFlee = canFlee;
        
        if (isMage)
        {
            MaxMana = Mana = 100f;
        }
    }

    public void ExecuteTurn(Player target)
    {
        if (ShouldFlee())
        {
            Flee();
            return;
        }

        if (ShouldRest())
        {
            Rest();
            return;
        }

        ChooseAction(target);
    }

    private void ChooseAction(Player target)
    {
        if (IsMage && Mana >= 10 && random.NextDouble() < 0.4)
        {
            CastSpecialSpell(target);
            return;
        }

        if (Stamina >= STAMINA_COST_PER_ATTACK)
        {
            if (random.NextDouble() < 0.3)
            {
                ApplyDebuff(target);
            }
            else
            {
                BasicAttack(target);
            }
            return;
        }

        Rest();
    }

    public void Attack(ICharacter target)
    {
        int damage = BattleSystem.CalculateMonsterAttack(this, (target as Player)?.Defense ?? 0);
        target.Hp -= damage;
        Stamina -= STAMINA_COST_PER_ATTACK;
    }

    private void BasicAttack(Player target)
    {
        Attack(target);
    }

    private void CastSpecialSpell(Player target)
    {
        if (Hp < MaxHp * 0.5 && random.NextDouble() < 0.4)
        {
            float healAmount = MaxHp * 0.3f;
            Hp = Math.Min(MaxHp, Hp + healAmount);
            Mana -= 10;
            Console.WriteLine($"{Name} heals for {healAmount} HP!");
        }
        else
        {
            float magicDamage = AttackPower * 1.5f;
            target.Hp -= magicDamage;
            Mana -= 15;
            Console.WriteLine($"{Name} casts a powerful spell for {magicDamage} damage!");
        }
    }

    private void ApplyDebuff(Player target)
    {
        string[] debuffTypes = { "fear", "slow", "weakness" };
        string selectedDebuff = debuffTypes[random.Next(debuffTypes.Length)];
        
        target.AddBuff(selectedDebuff, random.Next(2, 4));
        
        Stamina -= STAMINA_COST_PER_ATTACK;
        Console.WriteLine($"{Name} applies {selectedDebuff} to {target.Name}!");
    }

    private void Rest()
    {
        float staminaRecovery = MaxStamina * 0.2f;
        Stamina = Math.Min(MaxStamina, Stamina + staminaRecovery);
        Console.WriteLine($"{Name} rests and recovers {staminaRecovery} stamina!");
    }

    public void Defend()
    {
        Defense = (int)(Defense * 1.3);
        Stamina -= 1;
    }

    public void Flee()
    {
        if (Stamina >= 3)
        {
            Stamina -= 3;
        }
    }

    private bool ShouldFlee() =>
        CanFlee && Hp < MaxHp * FLEE_THRESHOLD && random.NextDouble() < 0.4;

    private bool ShouldRest() =>
        Stamina < MaxStamina * REST_THRESHOLD;
}
