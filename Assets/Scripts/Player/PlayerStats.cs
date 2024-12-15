using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class PlayerStats : MonoBehaviour
{
    [Header("Vital Stats")]
    public float currentHP;
    public float maxHP = 100f;
    public float currentMP;
    public float maxMP = 100f;
    public float currentStamina;
    public float maxStamina = 100f;

    [Header("Currency")]
    public int money;
    public int zunioCoins;

    [Header("Combat Stats")]
    public float attack;
    public float strength;
    public float defense;
    public float naturalDefense;
    public float criticalChance;
    public float agility;
    public float magicPower;

    [Header("Social Stats")]
    public float charisma;
    public float negotiation;
    public float nobility;
    
    [Header("Movement")]
    public float timeTravel = 1f; // Walking speed multiplier

    public event Action onDeath;
    public event Action<float> onHPChanged;
    public event Action<float> onMPChanged;
    public event Action<float> onStaminaChanged;

    private Dictionary<string, (float duration, float endTime)> activeBuffs = new Dictionary<string, (float, float)>();

    private void Start()
    {
        InitializeStats();
    }

    private void InitializeStats()
    {
        currentHP = maxHP;
        currentMP = maxMP;
        currentStamina = maxStamina;
    }

    public void ModifyHP(float amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        onHPChanged?.Invoke(currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void ModifyMP(float amount)
    {
        currentMP = Mathf.Clamp(currentMP + amount, 0, maxMP);
        onMPChanged?.Invoke(currentMP);
    }

    public void ModifyStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
        onStaminaChanged?.Invoke(currentStamina);
    }

    public void AddMoney(int amount)
    {
        money = Mathf.Max(0, money + amount);
    }

    public void AddZunioCoins(int amount)
    {
        zunioCoins = Mathf.Max(0, zunioCoins + amount);
    }

    public void IncreaseMaxHP(float amount)
    {
        maxHP += amount;
        currentHP += amount;
    }

    public void IncreaseMaxMP(float amount)
    {
        maxMP += amount;
        currentMP += amount;
    }

    public void IncreaseMaxStamina(float amount)
    {
        maxStamina += amount;
        currentStamina += amount;
    }

    private void Die()
    {
        onDeath?.Invoke();
        // Additional death logic here
    }

    public bool HasEnoughStamina(float cost)
    {
        return currentStamina >= cost;
    }

    public bool HasEnoughMP(float cost)
    {
        return currentMP >= cost;
    }

    public void ApplyDamage(float damage)
    {
        float finalDamage = damage - (defense + naturalDefense);
        finalDamage = Mathf.Max(1, finalDamage); // Minimum 1 damage
        ModifyHP(-finalDamage);
    }

    public void AddBuff(string buffName, float duration)
    {
        activeBuffs[buffName] = (duration, Time.time + duration);
    }

    public void AddStats(Item item)
    {
        strength += item.Strength;
        defense += item.Defense;
        // Add other stats as needed
    }

    public void RemoveStats(Item item)
    {
        strength -= item.Strength;
        defense -= item.Defense;
        // Remove other stats as needed
    }

    public void EquipItem(Item item)
    {
        AddStats(item);
    }

    private void Update()
    {
        UpdateBuffs();
    }

    private void UpdateBuffs()
    {
        var expiredBuffs = activeBuffs.Where(buff => Time.time > buff.Value.endTime).Select(buff => buff.Key).ToList();
        foreach (var buff in expiredBuffs)
        {
            activeBuffs.Remove(buff);
        }
    }
}
