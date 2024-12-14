using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using GoPath.Navigation;
using GoPath.UI;

namespace GoPath
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField]
        [Tooltip("Velocidade de movimento do personagem")]
        private float moveSpeed = 5f;

        [SerializeField]
        [Tooltip("Distância mínima para considerar que chegou ao destino")]
        private float arrivalThreshold = 0.05f; // Reduzido para maior precisão

        [SerializeField]
        [Tooltip("Precisão do posicionamento")]
        private float positionPrecision = 0.025f; // Reduzido para maior precisão

        [SerializeField]
        private bool showPathInGame = true;

        [SerializeField]
        private Color pathColor = Color.yellow;

        [Header("Navigation")]
        [SerializeField]
        [Tooltip("Referência ao GridManager que controla a navegação")]
        private GridManager gridManager;

        [Header("Debug")]
        [SerializeField]
        [Tooltip("Ativar/Desativar logs de debug")]
        private bool showDebugLogs = false;

        [Header("Target Visualization")]
        [SerializeField]
        [Tooltip("Cor do marcador que indica o destino")]
        private Color targetMarkerColor = new Color(1f, 0.5f, 0f, 0.8f);

        [SerializeField]
        [Tooltip("Tamanho do círculo que marca o destino")]
        private float targetMarkerRadius = 0.3f;

        [SerializeField]
        [Tooltip("Espessura da linha do círculo que marca o destino")]
        private float targetMarkerLineWidth = 0.1f;

        [Header("Visualization")]
        [SerializeField] private bool showPath = true;

        [Header("Integration Settings")]
        [SerializeField]
        [Tooltip("Componentes que podem bloquear o movimento")]
        private MonoBehaviour[] movementBlockers;

        private Rigidbody2D rb;
        private Camera mainCamera;
        private List<Vector2> currentPath;
        private int currentPathIndex;
        private bool isMoving;
        private GameObject targetMarker;
        private LineRenderer targetLine;
        private IMovementBlocker[] blockers;
        private bool isMovementBlocked;

        private void Start()
        {
            SetupComponents();
            SetupTargetMarker();
            if (gridManager != null)
            {
                gridManager.SetPathVisibility(showPath);
            }
            
            // Adicionar listener para o evento de bloqueio
            MovementBlocker.OnMovementBlockChanged += OnMovementBlockStateChanged;
        }

        private void OnDestroy()
        {
            MovementBlocker.OnMovementBlockChanged -= OnMovementBlockStateChanged;
        }

        private void OnMovementBlockStateChanged(bool blocked)
        {
            isMovementBlocked = blocked;
            if (blocked)
            {
                StopMovement();
            }
        }

        private void SetupComponents()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            mainCamera = Camera.main;

            if (gridManager == null)
            {
                gridManager = FindObjectOfType<GridManager>();
                if (gridManager == null)
                    LogError("GridManager não encontrado! Adicione-o à cena.");
            }

            // Setup blockers
            if (movementBlockers != null && movementBlockers.Length > 0)
            {
                blockers = movementBlockers
                    .Where(b => b is IMovementBlocker)
                    .Select(b => b as IMovementBlocker)
                    .ToArray();
            }
        }

        private void Update()
        {
            if (isMovementBlocked || MovementBlocker.IsBlocked)
                return;

            HandleInput();
            FollowPath();
        }

        private bool IsMovementBlocked()
        {
            if (blockers == null || blockers.Length == 0)
                return false;

            return blockers.Any(blocker => blocker.IsBlocking());
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                RequestPathToPosition(mousePosition);
            }
        }

        private void RequestPathToPosition(Vector2 targetPosition)
        {
            if (gridManager == null) return;

            Vector2? nearestTarget = gridManager.GetNearestWalkablePosition(targetPosition);
            if (!nearestTarget.HasValue)
            {
                LogWarning("Destino não navegável!");
                return;
            }

            Vector2? nearestStart = gridManager.GetNearestWalkablePosition(transform.position);
            if (!nearestStart.HasValue)
            {
                LogWarning("Posição inicial não navegável!");
                return;
            }

            List<Vector2> path = gridManager.FindPath(nearestStart.Value, nearestTarget.Value);
            if (path != null && path.Count > 0)
            {
                currentPath = path;
                currentPathIndex = 0;
                isMoving = true;
                DrawTargetMarker(path[path.Count - 1]);
                LogDebug($"Caminho encontrado com {path.Count} pontos");
            }
            else
            {
                LogWarning("Nenhum caminho encontrado!");
            }
        }

        private void FollowPath()
        {
            if (!isMoving || currentPath == null || currentPathIndex >= currentPath.Count)
            {
                StopMovement();
                return;
            }

            Vector2 targetPosition = currentPath[currentPathIndex];
            Vector2 currentPosition = transform.position;
            float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);

            if (distanceToTarget > arrivalThreshold)
            {
                MoveTowardsTarget(currentPosition, targetPosition);
            }
            else
            {
                currentPathIndex++;
                if (currentPathIndex >= currentPath.Count)
                {
                    ReachDestination();
                }
            }
        }

        private void MoveTowardsTarget(Vector2 current, Vector2 target)
        {
            float distanceToTarget = Vector2.Distance(current, target);
            Vector2 direction = (target - current).normalized;

            if (distanceToTarget < moveSpeed * Time.deltaTime)
            {
                transform.position = target;
                rb.velocity = Vector2.zero;
            }
            else
            {
                // Usar MovePosition para movimento mais preciso
                rb.MovePosition(current + direction * moveSpeed * Time.deltaTime);
            }

            if (showPathInGame)
            {
                Debug.DrawLine(current, target, pathColor);
            }
        }

        private void StopMovement()
        {
            rb.velocity = Vector2.zero;
            targetMarker.SetActive(false);
        }

        private void ReachDestination()
        {
            isMoving = false;
            StopMovement();
            LogDebug("Destino alcançado!");
        }

        private void SetupTargetMarker()
        {
            targetMarker = new GameObject("TargetMarker");
            targetLine = targetMarker.AddComponent<LineRenderer>();
            targetLine.startWidth = targetMarkerLineWidth;
            targetLine.endWidth = targetMarkerLineWidth;
            targetLine.material = new Material(Shader.Find("Sprites/Default"));
            targetLine.startColor = targetMarkerColor;
            targetLine.endColor = targetMarkerColor;
            targetMarker.SetActive(false);
        }

        private void DrawTargetMarker(Vector2 position)
        {
            const int segments = 30;
            targetLine.positionCount = segments + 1;
            
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector3 pos = new Vector3(
                    position.x + Mathf.Cos(angle) * targetMarkerRadius,
                    position.y + Mathf.Sin(angle) * targetMarkerRadius,
                    0
                );
                targetLine.SetPosition(i, pos);
            }
            
            targetMarker.SetActive(true);
        }

        private void LogDebug(string message)
        {
            if (showDebugLogs)
                Debug.Log($"[PlayerMovement] {message}");
        }

        private void LogWarning(string message)
        {
            if (showDebugLogs)
                Debug.LogWarning($"[PlayerMovement] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[PlayerMovement] {message}");
        }

        private void OnDrawGizmos()
        {
            if (!showPathInGame || currentPath == null || currentPath.Count == 0) 
                return;

            // Desenhar caminho atual
            Gizmos.color = pathColor;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
                Gizmos.DrawWireSphere(currentPath[i], positionPrecision);
            }
            Gizmos.DrawWireSphere(currentPath[currentPath.Count - 1], positionPrecision);
        }

        // Método para controlar a visibilidade do caminho
        public void TogglePathVisibility()
        {
            showPath = !showPath;
            if (gridManager != null)
            {
                gridManager.SetPathVisibility(showPath);
            }
        }
    }
}
