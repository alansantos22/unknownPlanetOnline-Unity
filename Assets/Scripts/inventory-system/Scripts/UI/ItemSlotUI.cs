using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private GameObject stackObject;

    private ItemSlot itemSlot;

    public void SetItemSlot(ItemSlot slot)
    {
        itemSlot = slot;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (itemSlot != null && itemSlot.Item != null)
        {
            itemIcon.sprite = itemSlot.Item.Icon;
            itemIcon.enabled = true;
            
            if (itemSlot.IsPiled && itemSlot.Quantity > 1)
            {
                stackObject.SetActive(true);
                quantityText.text = itemSlot.Quantity.ToString();
            }
            else
            {
                stackObject.SetActive(false);
            }
        }
        else
        {
            itemIcon.enabled = false;
            stackObject.SetActive(false);
        }
    }
}