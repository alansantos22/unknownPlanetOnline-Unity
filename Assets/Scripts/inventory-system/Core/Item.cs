using UnityEngine;
using System;
using InventorySystem.Enums;
using InventorySystem.Items; // Adicione esta linha

[Serializable]
public class Item
{
    public int ID { get; private set; }
    public string Name { get; private set; }
    public string Description;
    public Sprite Icon { get; private set; }
    public bool IsPiled { get; private set; }
    public int MaxStack { get; private set; } = 64;
    public int Price;
    public ItemType Type;
    public ItemRarity Rarity;

    public int HpRecovery;
    public int MpRecovery;
    public int StaminaRecovery;

    public int Attack;
    public int Strength;
    public int Dexterity;
    public int MagicPower;
    public int Vitality;
    public int Defense;

    public bool HasBoost;
    public string BoostEffect;
    public float BoostDuration;
    public bool UnlocksContent;
    public string UnlockableContent;

    public float AttackBoost;
    public float StrengthBoost;
    public float DefenseBoost;
    public float CriticalChanceBoost;
    public float AgilityBoost;
    public float MagicPowerBoost;

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

    public Item(ItemData itemData)
    {
        ID = itemData.id;
        Name = itemData.itemName;
        Description = itemData.description;
        Icon = itemData.icon;
        IsPiled = itemData.isStackable;
        MaxStack = itemData.maxStackSize;
        Price = itemData.price;
        Type = itemData.type;
        Rarity = itemData.rarity;
        
        HpRecovery = itemData.hpRecovery;
        MpRecovery = itemData.mpRecovery;
        StaminaRecovery = itemData.staminaRecovery;
        
        Attack = itemData.attack;
        Strength = itemData.strength;
        Defense = itemData.defense;
        MagicPower = itemData.MagicPower;
        
        HasBoost = itemData.hasBoost;
        BoostEffect = itemData.boostEffect;
        BoostDuration = itemData.boostDuration;
    }
}