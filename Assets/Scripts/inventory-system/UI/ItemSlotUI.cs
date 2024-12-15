using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[ExecuteInEditMode]
public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image quantityBackground;
    [SerializeField] private TextMeshProUGUI quantityText;
    
    [Header("Size Settings")]
    [ContextMenuItem("Apply Size Changes", "ApplySizeChanges")]
    [SerializeField] private Vector2 slotSize = new Vector2(100, 100);
    [SerializeField] private float iconSizePercent = 0.8f; // 80% do tamanho do slot
    
    [Header("Visual Settings")]
    [ContextMenuItem("Apply Visual Changes", "ApplyVisualChanges")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.8f, 0.8f, 0.8f);
    
    private ItemSlot itemSlot;

    private void Awake()
    {
        SetupReferences();
        ConfigureRectTransforms();
        InitializeUIState();
    }

    private void SetupReferences()
    {
        if (backgroundImage == null)
            backgroundImage = transform.Find("Background")?.GetComponent<Image>();
        
        if (iconImage == null)
            iconImage = transform.Find("Icon")?.GetComponent<Image>();
            
        if (quantityBackground == null)
            quantityBackground = transform.Find("QuantityBackground")?.GetComponent<Image>();
            
        if (quantityText == null)
            quantityText = quantityBackground?.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
    }

    private void ConfigureRectTransforms()
    {
        RectTransform slotRect = GetComponent<RectTransform>();
        slotRect.sizeDelta = slotSize;

        if (backgroundImage != null)
        {
            RectTransform bgRect = backgroundImage.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
        }

        if (iconImage != null)
        {
            float iconMargin = (1f - iconSizePercent) * 0.5f;
            RectTransform iconRect = iconImage.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(iconMargin, iconMargin);
            iconRect.anchorMax = new Vector2(1f - iconMargin, 1f - iconMargin);
            iconRect.sizeDelta = Vector2.zero;
        }

        // Removida a configuração específica do quantity background e text
        // Deixe essas configurações serem feitas no Unity Editor
    }

    private void InitializeUIState()
    {
        if (backgroundImage != null)
            backgroundImage.color = normalColor;
        if (iconImage != null)
            iconImage.enabled = false;
        if (quantityBackground != null)
            quantityBackground.enabled = false;
        if (quantityText != null)
            quantityText.enabled = false;
    }

    public void SetItemSlot(ItemSlot newItemSlot)
    {
        itemSlot = newItemSlot;
        UpdateUI();
    }

    public void ClearSlot()
    {
        itemSlot = null;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (itemSlot != null && itemSlot.Item != null)
        {
            iconImage.sprite = itemSlot.Item.Icon;
            iconImage.enabled = true;
            
            // Sempre mostra a quantidade quando há um item
            quantityBackground.enabled = true;
            quantityText.enabled = true;
            quantityText.text = itemSlot.Quantity.ToString();
        }
        else
        {
            iconImage.enabled = false;
            quantityBackground.enabled = false;
            quantityText.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }

    [ContextMenu("Apply All Changes")]
    public void ApplyAllChanges()
    {
        ConfigureRectTransforms();
        ApplyVisualChanges();
    }

    [ContextMenu("Apply Size Changes")]
    public void ApplySizeChanges()
    {
        ConfigureRectTransforms();
    }

    [ContextMenu("Apply Visual Changes")]
    public void ApplyVisualChanges()
    {
        if (backgroundImage != null)
            backgroundImage.color = normalColor;
        // Removida a configuração do quantity background e text
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Atualiza automaticamente quando valores são alterados no Inspector
        if (Application.isEditor && !Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    ApplyAllChanges();
                }
            };
        }
    }
#endif
}