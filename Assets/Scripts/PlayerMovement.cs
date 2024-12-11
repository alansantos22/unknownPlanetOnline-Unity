using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private LayerMask biomeLayer;
    [SerializeField] private float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Camera mainCamera;
    private Vector2 targetPosition;
    private bool isMoving;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        targetPosition = rb.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, 0f, biomeLayer);

            if (hit.collider != null)
            {
                targetPosition = mousePosition;
                isMoving = true;
            }
            else
            {
                Debug.Log("Can't move there - not a valid biome!");
            }
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            Vector2 currentPosition = rb.position;
            Vector2 moveDirection = (targetPosition - currentPosition).normalized;
            
            if (Vector2.Distance(currentPosition, targetPosition) > 0.1f)
            {
                rb.velocity = moveDirection * moveSpeed;
            }
            else
            {
                rb.velocity = Vector2.zero;
                isMoving = false;
            }
        }
    }
}