using UnityEngine;
using UnityEngine.EventSystems; // Adicionar este using

namespace UnknownPlanet
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private LayerMask biomeLayer;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float pathCheckDistance = 0.5f; // Distância entre cada verificação do caminho
        [SerializeField] private float stoppingDistance = 0.1f; // Nova variável para controlar distância de parada

        private Rigidbody2D rb;
        private Camera mainCamera;
        private Vector2 targetPosition;
        private bool isMoving;
        private ConstructionUI constructionUI; // Referência para o ConstructionUI

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            mainCamera = Camera.main;
            targetPosition = rb.position;
            constructionUI = FindObjectOfType<ConstructionUI>();
        }

        void Update()
        {
            // Não processa input se a UI de construção estiver aberta
            if (constructionUI != null && constructionUI.gameObject.activeInHierarchy)
                return;

            // Não processa input se o clique foi em um elemento da UI
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Collider2D[] hits = Physics2D.OverlapCircleAll(mousePosition, 0.1f);

                bool validBiome = false;
                foreach (Collider2D hit in hits)
                {
                    if (hit.CompareTag("Biome"))
                    {
                        validBiome = true;
                        Vector2 furthestValidPoint;
                        if (IsPathClear(rb.position, mousePosition, out furthestValidPoint))
                        {
                            targetPosition = mousePosition;
                        }
                        else
                        {
                            targetPosition = furthestValidPoint;
                        }
                        isMoving = true;
                        rb.velocity = Vector2.zero;
                        Debug.DrawLine(transform.position, targetPosition, Color.green, 1f);
                        break;
                    }
                }

                if (!validBiome)
                {
                    Debug.DrawLine(transform.position, mousePosition, Color.red, 1f);
                    Debug.Log("Can't move there - not on the biome!");
                }
            }
        }

        private bool IsPathClear(Vector2 start, Vector2 end, out Vector2 furthestValidPoint)
        {
            float distance = Vector2.Distance(start, end);
            Vector2 direction = (end - start).normalized;
            int steps = Mathf.Max(Mathf.CeilToInt(distance / pathCheckDistance), 1);
            
            furthestValidPoint = start;
            Vector2 lastValidPoint = start;

            for (int i = 0; i <= steps; i++) // Corrigido o erro de sintaxe aqui
            {
                float t = i / (float)steps;
                Vector2 checkPoint = Vector2.Lerp(start, end, t);
                Collider2D[] hits = Physics2D.OverlapCircleAll(checkPoint, 0.1f);
                
                bool validPoint = false;
                foreach (Collider2D hit in hits)
                {
                    if (hit.CompareTag("Biome"))
                    {
                        validPoint = true;
                        lastValidPoint = checkPoint;
                        break;
                    }
                }

                if (!validPoint)
                {
                    furthestValidPoint = lastValidPoint;
                    return false;
                }
            }

            furthestValidPoint = end;
            return true;
        }

        void FixedUpdate()
        {
            if (isMoving)
            {
                Vector2 currentPosition = rb.position;
                Vector2 moveDirection = (targetPosition - currentPosition).normalized;
                float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);
                
                if (distanceToTarget > stoppingDistance)
                {
                    float speedMultiplier = Mathf.Clamp01(distanceToTarget / 0.5f); // Suaviza a velocidade nos últimos 0.5 unidades
                    float currentSpeed = moveSpeed * speedMultiplier;
                    rb.velocity = moveDirection * currentSpeed;
                }
                else
                {
                    isMoving = false;
                    rb.velocity = Vector2.zero;
                }
            }
        }
    }
}
