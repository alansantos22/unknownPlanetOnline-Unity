using UnityEngine;
using InventorySystem.Enums;

public class ItemEffectHandler : MonoBehaviour
{
    public void HandleItemEffects(Item item, PlayerStats playerStats)
    {
        if (!item.IsPiled)
        {
            ApplyItemEffects(item, playerStats);
        }
    }

    private void ApplyItemEffects(Item item, PlayerStats playerStats)
    {
        // Recovery effects
        if (item.HpRecovery > 0) playerStats.ModifyHP(item.HpRecovery);
        if (item.MpRecovery > 0) playerStats.ModifyMP(item.MpRecovery);
        if (item.StaminaRecovery > 0) playerStats.ModifyStamina(item.StaminaRecovery);

        // Boost effects
        if (item.AttackBoost > 0) playerStats.AddBuff("Attack", item.BoostDuration);
        if (item.StrengthBoost > 0) playerStats.AddBuff("Strength", item.BoostDuration);
        if (item.DefenseBoost > 0) playerStats.AddBuff("Defense", item.BoostDuration);
        if (item.CriticalChanceBoost > 0) playerStats.AddBuff("CriticalChance", item.BoostDuration);
        if (item.AgilityBoost > 0) playerStats.AddBuff("Agility", item.BoostDuration);
        if (item.MagicPowerBoost > 0) playerStats.AddBuff("MagicPower", item.BoostDuration);

        // Type-specific effects
        if (item.Type == ItemType.Potion)
        {
            HandlePotionEffects(item, playerStats);
        }

        if (item.UnlocksSomething)
        {
            HandleUnlockableContent(item);
        }
    }

    private void HandlePotionEffects(Item item, PlayerStats playerStats)
    {
        // Logic for potions
    }

    private void HandleUnlockableContent(Item item)
    {
        // Logic to unlock features based on item
    }
}