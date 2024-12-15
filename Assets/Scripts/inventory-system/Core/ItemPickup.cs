using UnityEngine;
using InventorySystem.Items; // Adicione esta linha

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemData item;
    [SerializeField] private int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            playerStats.AddItem(item, amount);
            Destroy(gameObject);
        }
    }
}
