using UnityEngine;

namespace InventorySystem.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        public int id;
        public string itemName;
        public string description;
        public Sprite icon;
        public bool isPiled;
        public int maxStack = 64;
        public int price;
        public ItemType type;
        public ItemRarity rarity;

        [Header("Recovery Stats")]
        public int hpRecovery;
        public int mpRecovery;
        public int staminaRecovery;

        [Header("Equipment Stats")]
        public int strength;
        public int dexterity;
        public int intelligence;
        public int vitality;
        public int defense;

        [Header("Special Effects")]
        public bool hasBoost;
        public string boostEffect;
        public float boostDuration;
        public bool unlocksContent;
        public string unlockableContent;

        public Item CreateItem()
        {
            return new Item
            {
                ID = id,
                Name = itemName,
                Description = description,
                Icon = icon,
                IsPiled = isPiled,
                MaxStack = maxStack,
                Price = price,
                Type = type,
                Rarity = rarity,
                HpRecovery = hpRecovery,
                MpRecovery = mpRecovery,
                StaminaRecovery = staminaRecovery,
                Strength = strength,
                Dexterity = dexterity,
                Intelligence = intelligence,
                Vitality = vitality,
                Defense = defense,
                HasBoost = hasBoost,
                BoostEffect = boostEffect,
                BoostDuration = boostDuration,
                UnlocksContent = unlocksContent,
                UnlockableContent = unlockableContent
            };
        }
    }
}
