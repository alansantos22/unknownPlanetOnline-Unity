using UnityEngine;
using System.Collections.Generic;

public class BuffSystem : MonoBehaviour
{
    private Dictionary<string, BuffInfo> activeBuffs = new Dictionary<string, BuffInfo>();

    private void ApplyTemporaryBuff(string buffType, float value)
    {
        // Create or update buff info
        if (!activeBuffs.ContainsKey(buffType))
        {
            activeBuffs[buffType] = new BuffInfo { Value = value, Duration = 0f };
        }
    }

    public void ApplyItemBuffs(Item item, PlayerStats playerStats)
    {
        // Apply recovery effects
        if (item.HpRecovery > 0) playerStats.ModifyHP(item.HpRecovery);
        if (item.MpRecovery > 0) playerStats.ModifyMP(item.MpRecovery);
        if (item.StaminaRecovery > 0) playerStats.ModifyStamina(item.StaminaRecovery);

        // Apply stat boosts
        if (item.AttackBoost > 0) ApplyTemporaryBuff("Attack", item.AttackBoost);
        if (item.StrengthBoost > 0) ApplyTemporaryBuff("Strength", item.StrengthBoost);
        if (item.DefenseBoost > 0) ApplyTemporaryBuff("Defense", item.DefenseBoost);
        if (item.CriticalChanceBoost > 0) ApplyTemporaryBuff("CriticalChance", item.CriticalChanceBoost);
        if (item.AgilityBoost > 0) ApplyTemporaryBuff("Agility", item.AgilityBoost);
        if (item.MagicPowerBoost > 0) ApplyTemporaryBuff("MagicPower", item.MagicPowerBoost);
    }

    public void ApplyBuff(PlayerStats playerStats, Item item)
    {
        if (item == null) return;

        // Apply buffs based on item properties
        playerStats.currentHP += item.HpRecovery;
        playerStats.currentMP += item.MpRecovery;
        playerStats.currentStamina += item.StaminaRecovery;

        // Apply stat boosts
        playerStats.attack += item.AttackBoost;
        playerStats.strength += item.StrengthBoost;
        playerStats.defense += item.DefenseBoost;
        playerStats.criticalChance += item.CriticalChanceBoost;
        playerStats.agility += item.AgilityBoost;
        playerStats.magicPower += item.MagicPowerBoost;

        // Additional effects can be added here
    }

    public void RemoveBuff(PlayerStats playerStats, Item item)
    {
        if (item == null) return;

        // Remove buffs based on item properties
        playerStats.currentHP -= item.HpRecovery;
        playerStats.currentMP -= item.MpRecovery;
        playerStats.currentStamina -= item.StaminaRecovery;

        // Remove stat boosts
        playerStats.attack -= item.AttackBoost;
        playerStats.strength -= item.StrengthBoost;
        playerStats.defense -= item.DefenseBoost;
        playerStats.criticalChance -= item.CriticalChanceBoost;
        playerStats.agility -= item.AgilityBoost;
        playerStats.magicPower -= item.MagicPowerBoost;

        // Additional effects can be added here
    }
}

public class BuffInfo
{
    public float Value { get; set; }
    public float Duration { get; set; }
}