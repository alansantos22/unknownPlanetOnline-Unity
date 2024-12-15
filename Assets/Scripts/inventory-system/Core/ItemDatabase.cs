using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    private static ItemDatabase instance;
    public static ItemDatabase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ItemDatabase>();
            }
            return instance;
        }
    }

    private Dictionary<int, Item> items = new Dictionary<int, Item>();

    public void RegisterItem(Item item)
    {
        if (!items.ContainsKey(item.ID))
        {
            items.Add(item.ID, item);
        }
    }

    public Item GetItem(int id)
    {
        return items.ContainsKey(id) ? items[id] : null;
    }

    // Método para carregar itens do backend
    public void LoadItemsFromBackend(string jsonData)
    {
        // Implementar a lógica de deserialização do JSON do backend
        // e registrar os itens no database
    }
}