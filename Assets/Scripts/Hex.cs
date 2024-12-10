using UnityEngine;

public class Hex : MonoBehaviour
{
    public float noiseValue;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Color selectedColor = Color.red; // Color for the selected border
    private static Hex selectedHex; // Static reference to the currently selected hex

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Ensure the GameObject has a Collider2D component
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    public void Initialize(float noiseValue)
    {
        this.noiseValue = noiseValue;
        // Customize the hex based on noiseValue (e.g., change color or type)
    }

    void Update()
    {
        // Handle touch input for mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    OnTouchDown();
                }
            }
        }

        // Handle mouse input for testing in the editor
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                OnTouchDown();
            }
        }
    }

    private void OnTouchDown()
    {
        // Handle hex touch
        Debug.Log("Hex touched! Noise value: " + noiseValue + "; Coordinates: (" + transform.position.x + ", " + transform.position.y + ")");
        if (selectedHex != null && selectedHex != this)
        {
            selectedHex.Deselect();
        }
        selectedHex = this;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = selectedColor; // Change the border color to indicate selection
        }
    }

    private void Deselect()
    {
        // Reset the color when the hex is deselected
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}