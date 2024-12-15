using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem.Items; // Adicione esta linha

public class PlayerStats : MonoBehaviour
{
    [Header("Estatísticas Vitais")]
    [Tooltip("HP atual do jogador")]
    public float currentHP;
    [Tooltip("HP máximo do jogador")]
    public float maxHP = 100f;
    [Tooltip("MP atual do jogador")]
    public float currentMP;
    [Tooltip("MP máximo do jogador")]
    public float maxMP = 100f;
    [Tooltip("Stamina atual do jogador")]
    public float currentStamina;
    [Tooltip("Stamina máxima do jogador")]
    public float maxStamina = 100f;

    [Header("Moeda")]
    [Tooltip("Dinheiro do jogador")]
    public int money;
    [Tooltip("Moedas Zunio do jogador")]
    public int zunioCoins;

    [Header("Estatísticas de Combate")]
    [Tooltip("Poder de ataque do jogador")]
    public float attack;
    [Tooltip("Força do jogador")]
    public float strength;
    [Tooltip("Defesa do jogador")]
    public float defense;
    [Tooltip("Defesa natural do jogador")]
    public float naturalDefense;
    [Tooltip("Chance de crítico do jogador")]
    public float criticalChance;
    [Tooltip("Agilidade do jogador")]
    public float agility;
    [Tooltip("Poder mágico do jogador")]
    public float magicPower;

    [Header("Estatísticas Sociais")]
    [Tooltip("Carisma do jogador")]
    public float charisma;
    [Tooltip("Habilidade de negociação do jogador")]
    public float negotiation;
    [Tooltip("Nobreza do jogador")]
    public float nobility;
    
    [Header("Movimento")]
    [Tooltip("Multiplicador de velocidade de caminhada")]
    public float timeTravel = 1f; // Walking speed multiplier

    [Header("Inventário")]
    [Tooltip("Referência à UI do inventário")]
    [SerializeField] private InventoryUI inventoryUI;
    private Inventory inventory;

    [Tooltip("Item para teste")]
    [SerializeField] private ItemData itemParaTeste; // Adicione esta linha

    public event Action onDeath;
    public event Action<float> onHPChanged;
    public event Action<float> onMPChanged;
    public event Action<float> onStaminaChanged;

    private Dictionary<string, (float duration, float endTime)> activeBuffs = new Dictionary<string, (float, float)>();

    /// <summary>
    /// Initializes the player's stats when the component starts.
    /// </summary>
    private void Start()
    {
        InitializeStats();
        inventory = GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Inventory component not found on the player.");
        }
        if (inventoryUI == null)
        {
            Debug.LogError("InventoryUI reference not set on the player.");
        }
    }

    /// <summary>
    /// Sets initial values for HP, MP and Stamina to their maximum values.
    /// </summary>
    private void InitializeStats()
    {
        currentHP = maxHP;
        currentMP = maxMP;
        currentStamina = maxStamina;
    }

    /// <summary>
    /// Modifies the player's current HP and triggers related events.
    /// Calls Die() if HP reaches 0.
    /// </summary>
    /// <param name="amount">Amount to modify HP by (positive to heal, negative for damage)</param>
    public void ModifyHP(float amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        onHPChanged?.Invoke(currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Modifies the player's current MP and triggers related events.
    /// </summary>
    /// <param name="amount">Amount to modify MP by (positive to restore, negative to consume)</param>
    public void ModifyMP(float amount)
    {
        currentMP = Mathf.Clamp(currentMP + amount, 0, maxMP);
        onMPChanged?.Invoke(currentMP);
    }

    /// <summary>
    /// Modifies the player's current Stamina and triggers related events.
    /// </summary>
    /// <param name="amount">Amount to modify Stamina by (positive to restore, negative to consume)</param>
    public void ModifyStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
        onStaminaChanged?.Invoke(currentStamina);
    }

    /// <summary>
    /// Adds or removes money from the player's wallet. Cannot go below 0.
    /// </summary>
    /// <param name="amount">Amount of money to add or remove</param>
    public void AddMoney(int amount)
    {
        money = Mathf.Max(0, money + amount);
    }

    /// <summary>
    /// Adds or removes Zunio Coins from the player. Cannot go below 0.
    /// </summary>
    /// <param name="amount">Amount of Zunio Coins to add or remove</param>
    public void AddZunioCoins(int amount)
    {
        zunioCoins = Mathf.Max(0, zunioCoins + amount);
    }

    /// <summary>
    /// Increases maximum HP capacity and heals the player for the same amount.
    /// </summary>
    /// <param name="amount">Amount to increase max HP by</param>
    public void IncreaseMaxHP(float amount)
    {
        maxHP += amount;
        currentHP += amount;
    }

    /// <summary>
    /// Increases maximum MP capacity and restores MP by the same amount.
    /// </summary>
    /// <param name="amount">Amount to increase max MP by</param>
    public void IncreaseMaxMP(float amount)
    {
        maxMP += amount;
        currentMP += amount;
    }

    /// <summary>
    /// Increases maximum Stamina capacity and restores Stamina by the same amount.
    /// </summary>
    /// <param name="amount">Amount to increase max Stamina by</param>
    public void IncreaseMaxStamina(float amount)
    {
        maxStamina += amount;
        currentStamina += amount;
    }

    /// <summary>
    /// Handles the player's death, triggering the death event.
    /// </summary>
    private void Die()
    {
        onDeath?.Invoke();
        // Additional death logic here
    }

    /// <summary>
    /// Checks if the player has enough stamina for an action.
    /// </summary>
    /// <param name="cost">Amount of stamina required</param>
    /// <returns>True if player has enough stamina, false otherwise</returns>
    public bool HasEnoughStamina(float cost)
    {
        return currentStamina >= cost;
    }

    /// <summary>
    /// Checks if the player has enough MP for an action.
    /// </summary>
    /// <param name="cost">Amount of MP required</param>
    /// <returns>True if player has enough MP, false otherwise</returns>
    public bool HasEnoughMP(float cost)
    {
        return currentMP >= cost;
    }

    /// <summary>
    /// Calculates and applies damage to the player, considering defense stats.
    /// </summary>
    /// <param name="damage">Raw damage amount before defense calculation</param>
    public void ApplyDamage(float damage)
    {
        float finalDamage = damage - (defense + naturalDefense);
        finalDamage = Mathf.Max(1, finalDamage); // Minimum 1 damage
        ModifyHP(-finalDamage);
    }

    /// <summary>
    /// Adds a temporary buff to the player with specified duration.
    /// </summary>
    /// <param name="buffName">Name identifier for the buff</param>
    /// <param name="duration">How long the buff should last in seconds</param>
    public void AddBuff(string buffName, float duration)
    {
        activeBuffs[buffName] = (duration, Time.time + duration);
    }

    /// <summary>
    /// Adds item stats to the player's current stats.
    /// </summary>
    /// <param name="item">Item containing stats to add</param>
    public void AddStats(Item item)
    {
        strength += item.Strength;
        defense += item.Defense;
        // Add other stats as needed
    }

    /// <summary>
    /// Removes item stats from the player's current stats.
    /// </summary>
    /// <param name="item">Item containing stats to remove</param>
    public void RemoveStats(Item item)
    {
        strength -= item.Strength;
        defense -= item.Defense;
        // Remove other stats as needed
    }

    /// <summary>
    /// Equips an item and applies its stats to the player.
    /// </summary>
    /// <param name="item">Item to equip</param>
    public void EquipItem(Item item)
    {
        AddStats(item);
    }

    /// <summary>
    /// Adiciona um item ao inventário do jogador e atualiza a UI.
    /// </summary>
    /// <param name="item">Item a ser adicionado</param>
    /// <param name="amount">Quantidade do item a ser adicionada</param>
    public void AddItem(ItemData item, int amount = 1)
    {
        if (inventory != null)
        {
            inventory.AddItem(new Item(item), amount);
            if (inventoryUI != null)
            {
                inventoryUI.UpdateUI();
            }
        }
    }

    /// <summary>
    /// Updates the component every frame to check for expired buffs.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) // T para teste
        {
            AddItem(itemParaTeste, 1);
        }
        UpdateBuffs();
    }

    /// <summary>
    /// Removes any expired buffs from the active buffs dictionary.
    /// </summary>
    private void UpdateBuffs()
    {
        var expiredBuffs = activeBuffs.Where(buff => Time.time > buff.Value.endTime).Select(buff => buff.Key).ToList();
        foreach (var buff in expiredBuffs)
        {
            activeBuffs.Remove(buff);
        }
    }
}
