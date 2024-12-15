using UnityEngine;
using InventorySystem.Enums;

namespace InventorySystem.Items
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Informações Básicas")]
        [Tooltip("ID único do item")]
        public int id;
        [Tooltip("Nome do item")]
        public string itemName;
        [Tooltip("Ícone do item no inventário")]
        public Sprite icon;
        [TextArea(3, 10)]
        [Tooltip("Descrição detalhada do item")]
        public string description;
        [Tooltip("Define se o item pode ser empilhado")]
        public bool isStackable;
        [Tooltip("Quantidade máxima por pilha")]
        public int maxStackSize = 64;
        [Tooltip("Preço base do item")]
        public int price;
        [Tooltip("Categoria do item")]
        public ItemType type;
        [Tooltip("Raridade do item")]
        public ItemRarity rarity;

        [Header("Estatísticas de Recuperação")]
        [Tooltip("Quantidade de HP recuperado")]
        public int hpRecovery;
        [Tooltip("Quantidade de MP recuperado")]
        public int mpRecovery;
        [Tooltip("Quantidade de Stamina recuperada")]
        public int staminaRecovery;

        [Header("Estatísticas de Equipamento")]
        [Tooltip("Bônus de força")]
        public int strength;
        [Tooltip("Bônus de destreza")]
        public int dexterity;
        [Tooltip("Bônus de poder mágico")]
        public int MagicPower;
        [Tooltip("Bônus de vitalidade")]
        public int vitality;
        [Tooltip("Bônus de defesa")]
        public int defense;

        [Header("Efeitos Especiais")]
        [Tooltip("Indica se o item possui um efeito bônus")]
        public bool hasBoost;
        [Tooltip("Nome do efeito bônus")]
        public string boostEffect;
        [Tooltip("Duração do efeito em segundos")]
        public float boostDuration;
        [Tooltip("Indica se o item desbloqueia algum conteúdo")]
        public bool unlocksContent;
        [Tooltip("Conteúdo desbloqueável")]
        public string unlockableContent;

        [Header("Bônus Temporários")]
        [Tooltip("Poder de ataque base")]
        public int attack;
        [Tooltip("Bônus percentual ao ataque")]
        public float attackBoost;
        [Tooltip("Bônus percentual à força")]
        public float strengthBoost;
        [Tooltip("Bônus percentual à defesa")]
        public float defenseBoost;
        [Tooltip("Bônus à chance de crítico")]
        public float criticalChanceBoost;
        [Tooltip("Bônus à agilidade")]
        public float agilityBoost;
        [Tooltip("Bônus ao poder mágico")]
        public float magicPowerBoost;
    }
}