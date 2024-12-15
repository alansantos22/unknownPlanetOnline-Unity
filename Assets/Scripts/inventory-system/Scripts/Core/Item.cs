using UnityEngine;
using System;
using InventorySystem.Enums;

[Serializable]
public class Item
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Sprite Icon { get; set; }
    public bool IsPiled { get; set; }
    public int MaxStack { get; set; } = 64;
    public int Price { get; set; }
    public ItemType Type { get; set; }
    public ItemRarity Rarity { get; set; }

    // Recovery stats
    public int HpRecovery { get; set; }
    public int MpRecovery { get; set; }
    public int StaminaRecovery { get; set; }

    // Equipment stats
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Intelligence { get; set; }
    public int Vitality { get; set; }
    public int Defense { get; set; }

    // Special effects
    public bool HasBoost { get; set; }
    public string BoostEffect { get; set; }
    public float BoostDuration { get; set; }
    public bool UnlocksContent { get; set; }
    public string UnlockableContent { get; set; }

    // Boost stats
    public float AttackBoost { get; set; }
    public float StrengthBoost { get; set; }
    public float DefenseBoost { get; set; }
    public float CriticalChanceBoost { get; set; }
    public float AgilityBoost { get; set; }
    public float MagicPowerBoost { get; set; }

    // Equipment specific
    public bool IsEquippable => Type == ItemType.Weapon || 
                               Type == ItemType.Armor || 
                               Type == ItemType.Gauntlet ||
                               Type == ItemType.Boots ||
                               Type == ItemType.Helmet ||
                               Type == ItemType.Ring ||
                               Type == ItemType.Necklace ||
                               Type == ItemType.Mount ||
                               Type == ItemType.Pet;

    public bool IsConsumable => Type == ItemType.Potion || 
                               (Type == ItemType.Loot && 
                               (HpRecovery > 0 || MpRecovery > 0 || StaminaRecovery > 0 || HasBoost));

    public bool CanBeUsedInOffHand => Type == ItemType.Weapon && IsPiled == false;

    public bool UnlocksSomething => UnlocksContent;

    public void Use(PlayerStats playerStats)
    {
        if (IsConsumable)
        {
            ApplyConsumableEffects(playerStats);
        }
        else if (IsEquippable)
        {
            playerStats.EquipItem(this);
        }
    }

    private void ApplyConsumableEffects(PlayerStats playerStats)
    {
        playerStats.ModifyHP(HpRecovery);
        playerStats.ModifyMP(MpRecovery);
        playerStats.ModifyStamina(StaminaRecovery);

        if (HasBoost)
        {
            playerStats.AddBuff(BoostEffect, BoostDuration);
        }
    }
}