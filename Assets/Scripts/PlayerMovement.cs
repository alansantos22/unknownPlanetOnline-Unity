using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq; // Adicionar para usar Any()

namespace UnknownPlanet
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Velocidade de movimento do personagem")]
        private float moveSpeed = 5f;

        [SerializeField]
        [Tooltip("Distância para verificar obstáculos durante o movimento")]
        private float pathCheckDistance = 0.5f;

        [SerializeField]
        [Tooltip("Distância entre os pontos da grade de navegação")]
        private float nodeDistance = 1f;

        [SerializeField]
        [Tooltip("Raio do círculo usado para detectar colisões")]
        private float raycastRadius = 0.1f;

        [SerializeField]
        [Tooltip("Distância mínima para considerar que chegou ao destino")]
        private float arrivalThreshold = 0.1f;

        [Header("Navigation Settings")]
        [SerializeField]
        [Tooltip("Terreno onde o personagem pode andar")]
        private GameObject walkableTerrain;

        [SerializeField]
        [Tooltip("Lista de obstáculos que bloqueiam o movimento (água, etc)")]
        private GameObject[] obstacles;

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

        // Add new debug settings
        [Header("Debug Settings")]
        [SerializeField]
        [Tooltip("Ativar/Desativar logs de debug no console")]
        private bool showDebugLogs = false;

        [SerializeField]
        [Tooltip("Ativar/Desativar recálculo automático de rota ao encontrar obstáculos")]
        private bool enablePathRecalculation = true;

        private Rigidbody2D rb;
        private Camera mainCamera;
        private ConstructionUI constructionUI;
        private List<Vector2> path = new List<Vector2>();
        private int currentPathIndex;
        private bool isMoving;

        private GameObject targetMarker;
        private LineRenderer targetLine;

        private class PathNode
        {
            public Vector2 position;
            public float gCost; // Custo do início até este nó
            public float hCost; // Custo estimado deste nó até o destino (heurística)
            public float fCost => gCost + hCost;
            public PathNode parent;
            
            public PathNode(Vector2 pos)
            {
                position = pos;
                gCost = float.MaxValue;
                hCost = 0;
                parent = null;
            }
        }

        private class PriorityQueue<T>
        {
            private List<(T item, float priority)> elements = new List<(T, float)>();

            public int Count => elements.Count;

            public void Enqueue(T item, float priority)
            {
                elements.Add((item, priority));
                int ci = elements.Count - 1;
                while (ci > 0)
                {
                    int pi = (ci - 1) / 2;
                    if (elements[ci].priority >= elements[pi].priority) break;
                    var tmp = elements[ci];
                    elements[ci] = elements[pi];
                    elements[pi] = tmp;
                    ci = pi;
                }
            }

            public T Dequeue()
            {
                int li = elements.Count - 1;
                T frontItem = elements[0].item;
                elements[0] = elements[li];
                elements.RemoveAt(li);

                li--;
                int pi = 0;
                while (true)
                {
                    int ci = pi * 2 + 1;
                    if (ci > li) break;
                    int rc = ci + 1;
                    if (rc <= li && elements[rc].priority < elements[ci].priority)
                        ci = rc;
                    if (elements[pi].priority <= elements[ci].priority) break;
                    var tmp = elements[pi];
                    elements[pi] = elements[ci];
                    elements[ci] = tmp;
                    pi = ci;
                }
                return frontItem;
            }
        }

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            mainCamera = Camera.main;
            constructionUI = FindObjectOfType<ConstructionUI>();

            SetupTargetMarker();
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

        void Update()
        {
            if (constructionUI != null && constructionUI.gameObject.activeInHierarchy)
                return;

            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                FindPath(mousePosition);
            }

            FollowPath();
        }

        private float CalculateHCost(Vector2 start, Vector2 end)
        {
            // Usando distância Manhattan para grade
            return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y);
        }

        private void FindPath(Vector2 target)
        {
            path.Clear();
            currentPathIndex = 0;

            Vector2 startPos = transform.position;
            if (!IsPositionWalkable(startPos) || !IsPositionWalkable(target))
            {
                LogWarning("Invalid start or end position");
                return;
            }

            var openSet = new PriorityQueue<PathNode>();
            var closedSet = new HashSet<Vector2>();
            var startNode = new PathNode(startPos);
            startNode.gCost = 0;
            startNode.hCost = CalculateHCost(startPos, target);
            
            openSet.Enqueue(startNode, startNode.fCost);

            while (openSet.Count > 0)
            {
                var currentNode = openSet.Dequeue();
                
                if (Vector2.Distance(currentNode.position, target) < nodeDistance)
                {
                    ReconstructPath(currentNode);
                    isMoving = true;
                    DrawTargetMarker(target);
                    return;
                }

                closedSet.Add(currentNode.position);

                foreach (var neighbor in GetNeighbors(currentNode.position))
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    float tentativeGCost = currentNode.gCost + Vector2.Distance(currentNode.position, neighbor);
                    var neighborNode = new PathNode(neighbor);
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = CalculateHCost(neighbor, target);
                    neighborNode.parent = currentNode;
                    openSet.Enqueue(neighborNode, neighborNode.fCost);
                }
            }

            LogWarning("No path found");
        }

        private List<Vector2> GetNeighbors(Vector2 position)
        {
            var neighbors = new List<Vector2>();
            float[] angles = { 0, 45, 90, 135, 180, 225, 270, 315 };

            foreach (float angle in angles)
            {
                Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
                Vector2 neighborPos = position + direction * nodeDistance;

                if (IsPositionWalkable(neighborPos))
                    neighbors.Add(neighborPos);
            }

            return neighbors;
        }

        private bool IsPositionWalkable(Vector2 position)
        {
            // Verificar se está sobre algum obstáculo
            foreach (var obstacle in obstacles)
            {
                if (obstacle != null && obstacle.GetComponent<Collider2D>()?.OverlapPoint(position) == true)
                {
                    LogDebug($"Position {position} blocked by {obstacle.name}");
                    return false;
                }
            }

            // Verificar se está sobre terreno válido
            if (walkableTerrain != null && walkableTerrain.GetComponent<Collider2D>()?.OverlapPoint(position) == true)
            {
                LogDebug($"Found walkable position at {position}");
                return true;
            }

            LogDebug($"No walkable ground at {position}");
            return false;
        }

        private bool HasClearPath(Vector2 start, Vector2 end)
        {
            // Verificar colisão com cada obstáculo
            foreach (var obstacle in obstacles)
            {
                if (obstacle != null)
                {
                    var hit = Physics2D.Linecast(start, end, 
                        LayerMask.GetMask(LayerMask.LayerToName(obstacle.layer)));
                    if (hit.collider != null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void ReconstructPath(PathNode endNode)
        {
            path.Clear();
            var current = endNode;

            while (current != null)
            {
                path.Add(current.position);
                current = current.parent;
            }
            
            path.Reverse();
            
            // Otimizar o caminho removendo nós desnecessários
            OptimizePath();

            if (path.Count > 0)
            {
                LogDebug($"Path found with {path.Count} nodes");
            }
        }

        private void OptimizePath()
        {
            if (path.Count <= 2) return;

            var optimizedPath = new List<Vector2> { path[0] };
            int current = 0;

            while (current < path.Count - 1)
            {
                int furthest = current + 1;
                
                for (int check = current + 2; check < path.Count; check++)
                {
                    if (HasClearPath(path[current], path[check]))
                    {
                        furthest = check;
                    }
                }

                optimizedPath.Add(path[furthest]);
                current = furthest;
            }

            path = optimizedPath;
        }

        private void FollowPath()
        {
            if (!isMoving || currentPathIndex >= path.Count)
            {
                rb.velocity = Vector2.zero;
                targetMarker.SetActive(false);  // Hide marker when stopped
                return;
            }

            Vector2 currentTarget = path[currentPathIndex];
            Vector2 currentPosition = transform.position;
            float distanceToTarget = Vector2.Distance(currentPosition, currentTarget);

            // Check if path is still valid
            if (enablePathRecalculation && !HasClearPath(currentPosition, currentTarget))
            {
                LogDebug("Obstacle detected! Recalculating path...");
                FindPath(path[path.Count - 1]); // Recalculate to final destination
                return;
            }

            LogDebug($"Following path: Position={currentPosition}, Target={currentTarget}, Distance={distanceToTarget:F2}");

            if (distanceToTarget > arrivalThreshold)
            {
                Vector2 direction = (currentTarget - currentPosition).normalized;
                rb.velocity = direction * moveSpeed;
                Debug.DrawLine(currentPosition, currentTarget, Color.yellow);
            }
            else
            {
                currentPathIndex++;
                if (currentPathIndex >= path.Count)
                {
                    isMoving = false;
                    rb.velocity = Vector2.zero;
                    LogDebug("Reached final destination");
                }
                else
                {
                    LogDebug($"Moving to next point: {path[currentPathIndex]}");
                }
            }
        }

        // Adicione este método para debug visual
        private void OnDrawGizmos()
        {
            if (path != null && path.Count > 0)
            {
                Gizmos.color = Color.green;
                foreach (var point in path)
                {
                    Gizmos.DrawWireSphere(point, 0.2f);
                }
            }
        }

        private void OnDestroy()
        {
            if (targetMarker != null)
                Destroy(targetMarker);
        }

        private void LogDebug(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log(message);
            }
        }

        private void LogWarning(string message)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning(message);
            }
        }

        // Adicionar detecção de colisão para recálculo de rota
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (enablePathRecalculation && isMoving)
            {
                foreach (var obstacle in obstacles)
                {
                    if (collision.gameObject == obstacle)
                    {
                        LogDebug($"Collision with obstacle {obstacle.name}! Recalculating path...");
                        FindPath(path[path.Count - 1]);
                        break;
                    }
                }
            }
        }
    }

    // Adicionar método de extensão para debug visual
    public static class DebugExtension
    {
        public static void DrawCircle(Vector2 center, float radius, Color color, float duration)
        {
            int segments = 20;
            float angle = 360f / segments;
            
            for (int i = 0; i < segments; i++)
            {
                float currentAngle = angle * i * Mathf.Deg2Rad;
                float nextAngle = angle * (i + 1) * Mathf.Deg2Rad;
                
                Vector2 currentPoint = center + new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * radius;
                Vector2 nextPoint = center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * radius;
                
                Debug.DrawLine(currentPoint, nextPoint, color, duration);
            }
        }
    }
}
