using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float edgeScrollThreshold = 20f;
    [SerializeField] private bool enableEdgeScrolling = true;

    [Header("Boundaries")]
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX = 50f;
    [SerializeField] private float minY = -50f;
    [SerializeField] private float maxY = 50f;

    [Header("Map Reference")]
    [SerializeField] private SpriteRenderer mapBounds;
    [SerializeField] private bool useMapBounds = true;
    [SerializeField] private float padding = 1f; // Espaço extra nas bordas

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float minZoom = 2f;  // Zoom máximo (mais próximo)
    [SerializeField] private float maxZoom = 10f; // Zoom mínimo (mais distante)
    [SerializeField] private bool smoothZoom = true;
    [SerializeField] private float zoomSmoothness = 10f;

    private Vector3 dragOrigin;
    private bool isDragging = false;
    private Camera mainCamera;
    private float targetZoom;

    private void Start()
    {
        mainCamera = Camera.main;
        targetZoom = mainCamera.orthographicSize;
        if (useMapBounds && mapBounds != null)
        {
            SetupMapBounds();
        }
    }

    private void Update()
    {
        HandleKeyboardInput();
        HandleMouseDrag();
        HandleZoom();
        if (enableEdgeScrolling) HandleEdgeScrolling();
        ClampPosition();
    }

    private void HandleKeyboardInput()
    {
        Vector2 moveDirection = Vector2.zero;

        // Arrow keys
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            moveDirection += Vector2.up;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            moveDirection += Vector2.down;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            moveDirection += Vector2.left;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            moveDirection += Vector2.right;

        transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime, Space.World);
    }

    public void ResetDragState()
    {
        isDragging = false;
    }

    private void HandleMouseDrag()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            isDragging = false;
            return;
        }

        // Handle touch input for mobile and mouse drag for desktop
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragOrigin = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 currentMousePos = Input.mousePosition;
            Vector3 difference = dragOrigin - currentMousePos;
            transform.position += new Vector3(
                difference.x * moveSpeed * Time.deltaTime * 0.01f,
                difference.y * moveSpeed * Time.deltaTime * 0.01f,
                0
            );
            dragOrigin = currentMousePos;
        }
    }

    private void HandleEdgeScrolling()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 moveDirection = Vector2.zero;

        // Check if mouse is near screen edges
        if (mousePos.x < edgeScrollThreshold)
            moveDirection += Vector2.left;
        else if (mousePos.x > Screen.width - edgeScrollThreshold)
            moveDirection += Vector2.right;

        if (mousePos.y < edgeScrollThreshold)
            moveDirection += Vector2.down;
        else if (mousePos.y > Screen.height - edgeScrollThreshold)
            moveDirection += Vector2.up;

        transform.Translate(moveDirection.normalized * moveSpeed * 0.5f * Time.deltaTime, Space.World);
    }

    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (scrollInput != 0)
        {
            targetZoom = Mathf.Clamp(targetZoom - scrollInput * zoomSpeed, minZoom, maxZoom);
        }

        if (smoothZoom)
        {
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, Time.deltaTime * zoomSmoothness);
        }
        else
        {
            mainCamera.orthographicSize = targetZoom;
        }

        // Atualiza os limites do mapa baseado no novo zoom, se estiver usando mapBounds
        if (useMapBounds && mapBounds != null)
        {
            SetupMapBounds();
        }
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    private void SetupMapBounds()
    {
        // Calcula os limites baseado no tamanho do sprite do mapa
        float vertExtent = mainCamera.orthographicSize;
        float horizExtent = vertExtent * Screen.width / Screen.height;

        Bounds bounds = mapBounds.bounds;
        minX = bounds.min.x + horizExtent + padding;
        maxX = bounds.max.x - horizExtent - padding;
        minY = bounds.min.y + vertExtent + padding;
        maxY = bounds.max.y - vertExtent - padding;
    }
}