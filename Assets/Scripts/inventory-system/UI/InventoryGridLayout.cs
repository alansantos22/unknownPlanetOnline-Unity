using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(GridLayoutGroup))]
public class InventoryGridLayout : MonoBehaviour
{
    [Header("Grid Settings")]
    [ContextMenuItem("Apply Grid Changes", "ApplyGridChanges")]
    [SerializeField] private int columns = 5;
    [SerializeField] private Vector2 slotSize = new Vector2(100, 100);
    [SerializeField] private Vector2 spacing = new Vector2(15, 15);
    [SerializeField] private TextAnchor alignment = TextAnchor.UpperLeft;
    
    private GridLayoutGroup grid;

    private void OnEnable()
    {
        grid = GetComponent<GridLayoutGroup>();
        ConfigureGrid();
    }

    [ContextMenu("Apply Grid Changes")]
    public void ApplyGridChanges()
    {
        ConfigureGrid();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    ApplyGridChanges();
                }
            };
        }
    }
#endif

    private void ConfigureGrid()
    {
        if (grid != null)
        {
            grid.cellSize = slotSize;
            grid.spacing = spacing;
            
            grid.padding = new RectOffset(20, 20, 20, 20);
            
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.childAlignment = alignment;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
    }

    public void UpdateColumns(int newColumns)
    {
        columns = newColumns;
        if (grid != null)
        {
            grid.constraintCount = columns;
        }
    }

    public void UpdateSpacing(Vector2 newSpacing)
    {
        spacing = newSpacing;
        if (grid != null)
        {
            grid.spacing = spacing;
        }
    }
}
