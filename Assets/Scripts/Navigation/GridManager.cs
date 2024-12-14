using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnknownPlanet.Navigation
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField]
        [Tooltip("Distância entre os pontos da malha")]
        private float gridSpacing = 0.25f; // Reduzido para maior precisão

        [SerializeField]
        [Tooltip("Margem de segurança para bordas e obstáculos")]
        private float safetyMargin = 0.3f;

        [SerializeField]
        [Tooltip("Camada que representa áreas onde é possível andar")]
        private GameObject walkableArea;

        [SerializeField]
        [Tooltip("Objetos que representam obstáculos")]
        private GameObject[] obstacles;

        [Header("Debug")]
        [SerializeField]
        [Tooltip("Mostrar grid de navegação")]
        private bool showGrid = true;

        [SerializeField]
        [Tooltip("Cor dos pontos navegáveis")]
        private Color walkableColor = new Color(0, 1, 0, 0.3f);

        [Header("Grid Data")]
        [SerializeField]
        [Tooltip("Dados pré-calculados da malha de navegação")]
        private GridData gridData;

        [SerializeField]
        [Tooltip("Modo de edição para gerar nova malha")]
        private bool editMode = false;

        [Header("Visualization")]
        [SerializeField]
        [Tooltip("Mostra os nós da malha de navegação durante o jogo")]
        private bool showNodesInGame = false;

        [SerializeField]
        [Tooltip("Mostra as conexões entre os nós durante o jogo")]
        private bool showConnectionsInGame = false;

        [SerializeField]
        [Tooltip("Cor dos nós da malha de navegação")]
        private Color nodeColor = new Color(0, 1, 0, 0.5f);

        [SerializeField]
        [Tooltip("Cor das conexões entre os nós")]
        private Color connectionColor = new Color(0, 0.5f, 0, 0.2f);

        [SerializeField]
        [Tooltip("Tamanho base dos nós (será ajustado pelo sistema de escala)")]
        private float nodeSize = 0.1f;

        [SerializeField]
        [Tooltip("Mostra a linha do caminho encontrado")]
        private bool showPathLine = true;

        [SerializeField]
        [Tooltip("Cor do caminho encontrado")]
        private Color pathColor = Color.yellow;

        [SerializeField]
        [Tooltip("Largura base da linha do caminho (será ajustada pelo sistema de escala)")]
        private float pathLineWidth = 0.1f;

        [Header("Debug Visualization")]
        [SerializeField]
        [Tooltip("Ativa a visualização de debug geral")]
        private bool showDebugGrid = true;

        [SerializeField]
        [Tooltip("Mostra os nós da malha no modo debug")]
        private bool showNodes = true;

        [SerializeField]
        [Tooltip("Mostra as conexões entre os nós no modo debug")]
        private bool showConnections = true;

        [SerializeField]
        [Tooltip("Mostra as bordas da área navegável")]
        private bool showBounds = true;

        [SerializeField]
        [Tooltip("Cor das bordas da área navegável")]
        private Color boundsColor = new Color(1f, 1f, 0f, 0.3f);

        [SerializeField]
        [Tooltip("Largura base das conexões (será ajustada pelo sistema de escala)")]
        private float connectionWidth = 0.05f;

        [Header("Size Adjustment")]
         [SerializeField]
        [Tooltip("Ativa o ajuste automático do tamanho dos elementos visuais baseado no tamanho do mapa")]
        private bool autoAdjustSize = true;

        [SerializeField]
        [Tooltip("Multiplicador geral para ajuste fino do tamanho dos elementos visuais")]
        private float sizeMultiplier = 1f;

        [SerializeField]
        [Tooltip("Tamanho base do mapa considerado 'normal' (em unidades). Mapas menores terão elementos maiores e vice-versa")]
        private float referenceMapSize = 10f; // Tamanho de referência para um mapa "normal"
        private float mapScaleFactor = 1f;

        private Dictionary<Vector2Int, NavigationNode> navigationGrid;
        private Bounds mapBounds;
        private LineRenderer pathVisualizer;
        private List<Vector2> currentDebugPath;

        private void Awake()
        {
            SetupVisualizer();
        }

        private void Start()
        {
            if (editMode)
            {
                InitializeGrid();
                SaveGridData();
            }
            else if (gridData != null)
            {
                LoadGridData();
            }

            CalculateMapScale();
        }

        private void SetupVisualizer()
        {
            var visualizer = new GameObject("PathVisualizer");
            visualizer.transform.SetParent(transform);
            pathVisualizer = visualizer.AddComponent<LineRenderer>();
            pathVisualizer.startWidth = pathLineWidth * mapScaleFactor;
            pathVisualizer.endWidth = pathLineWidth * mapScaleFactor;
            pathVisualizer.material = new Material(Shader.Find("Sprites/Default"));
            pathVisualizer.startColor = pathColor;
            pathVisualizer.endColor = pathColor;
            pathVisualizer.enabled = showPathLine;
        }

        private void InitializeGrid()
        {
            if (walkableArea == null) return;

            var collider = walkableArea.GetComponent<Collider2D>();
            if (collider == null) return;

            mapBounds = collider.bounds;
            navigationGrid = new Dictionary<Vector2Int, NavigationNode>();

            // Aumentar a densidade de pontos de verificação
            float halfSpacing = gridSpacing * 0.5f;
            
            for (float x = mapBounds.min.x; x <= mapBounds.max.x; x += halfSpacing)
            {
                for (float y = mapBounds.min.y; y <= mapBounds.max.y; y += halfSpacing)
                {
                    Vector2 worldPos = new Vector2(x, y);
                    if (IsStrictlyWalkable(worldPos))
                    {
                        Vector2Int gridPos = WorldToGridPosition(worldPos);
                        if (!navigationGrid.ContainsKey(gridPos))
                        {
                            navigationGrid[gridPos] = new NavigationNode(worldPos);
                        }
                    }
                }
            }

            ConnectNodes();
            CleanupIsolatedNodes();
        }

        private bool IsStrictlyWalkable(Vector2 position)
        {
            var walkableCollider = walkableArea.GetComponent<Collider2D>();
            
            // Primeiro, verifica se o ponto está dentro da área walkable
            if (!walkableCollider.OverlapPoint(position))
                return false;

            // Verifica se há pontos ao redor também dentro da área walkable
            float checkRadius = gridSpacing * 0.5f;
            Vector2[] checkPoints = new Vector2[]
            {
                position + Vector2.up * checkRadius,
                position + Vector2.down * checkRadius,
                position + Vector2.left * checkRadius,
                position + Vector2.right * checkRadius
            };

            foreach (var point in checkPoints)
            {
                if (!walkableCollider.OverlapPoint(point))
                    return false;
            }

            // Verifica distância dos obstáculos
            foreach (var obstacle in obstacles)
            {
                if (obstacle != null)
                {
                    var obstacleCollider = obstacle.GetComponent<Collider2D>();
                    if (obstacleCollider != null)
                    {
                        float distance = Vector2.Distance(position, 
                            obstacleCollider.ClosestPoint(position));
                        if (distance < safetyMargin)
                            return false;
                    }
                }
            }

            return true;
        }

        private void CleanupIsolatedNodes()
        {
            var nodesToRemove = new HashSet<Vector2Int>();

            foreach (var pair in navigationGrid)
            {
                if (pair.Value.Connections.Count == 0)
                {
                    nodesToRemove.Add(pair.Key);
                }
            }

            foreach (var key in nodesToRemove)
            {
                navigationGrid.Remove(key);
            }
        }

        public List<Vector2> FindPath(Vector2 start, Vector2 end)
        {
            // Encontrar nós mais próximos do início e fim
            Vector2Int startGrid = WorldToGridPosition(start);
            Vector2Int endGrid = WorldToGridPosition(end);

            if (!navigationGrid.TryGetValue(startGrid, out NavigationNode startNode) || 
                !navigationGrid.TryGetValue(endGrid, out NavigationNode endNode))
            {
                return null;
            }

            // Resetar todos os nós
            foreach (var node in navigationGrid.Values)
            {
                node.Reset();
            }

            var openSet = new List<NavigationNode>();
            var closedSet = new HashSet<NavigationNode>();

            startNode.GCost = 0;
            startNode.HCost = Vector2.Distance(start, end);
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                var currentNode = openSet.OrderBy(n => n.FCost).First();

                if (currentNode == endNode)
                {
                    var path = ReconstructPath(endNode);
                    ShowPathVisualization(path);
                    return path;
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                foreach (var neighbor in currentNode.Connections)
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    float tentativeGCost = currentNode.GCost + 
                        Vector2.Distance(currentNode.WorldPosition, neighbor.WorldPosition);

                    if (tentativeGCost < neighbor.GCost)
                    {
                        neighbor.Parent = currentNode;
                        neighbor.GCost = tentativeGCost;
                        neighbor.HCost = Vector2.Distance(neighbor.WorldPosition, end);

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            return null;
        }

        private void ShowPathVisualization(List<Vector2> path)
        {
            if (!showPathLine || pathVisualizer == null)
            {
                return;
            }

            if (path == null || path.Count == 0)
            {
                pathVisualizer.positionCount = 0;
                return;
            }

            pathVisualizer.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                pathVisualizer.SetPosition(i, new Vector3(path[i].x, path[i].y, 0));
            }
        }

        private void ConnectNodes()
        {
            foreach (var node in navigationGrid.Values)
            {
                Vector2Int gridPos = WorldToGridPosition(node.WorldPosition);
                
                // Verificar 8 direções
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;

                        Vector2Int neighborPos = new Vector2Int(gridPos.x + x, gridPos.y + y);
                        if (navigationGrid.TryGetValue(neighborPos, out NavigationNode neighbor))
                        {
                            // Verificar se o caminho direto é seguro
                            if (IsPathSafe(node.WorldPosition, neighbor.WorldPosition))
                            {
                                node.AddConnection(neighbor);
                            }
                        }
                    }
                }
            }
        }

        private bool IsPathSafe(Vector2 start, Vector2 end)
        {
            float distance = Vector2.Distance(start, end);
            int checks = Mathf.Max(10, Mathf.CeilToInt(distance / (gridSpacing * 0.1f)));
            
            // Verificar se os pontos inicial e final estão dentro da área
            if (!IsStrictlyWalkable(start) || !IsStrictlyWalkable(end))
                return false;

            Vector2 pathDirection = (end - start).normalized;
            float checkDistance = safetyMargin * 0.5f;

            for (int i = 0; i <= checks; i++)
            {
                Vector2 point = Vector2.Lerp(start, end, i / (float)checks);
                Vector2 perpendicular = Vector2.Perpendicular(pathDirection) * checkDistance;

                // Verificar o ponto central e pontos laterais
                if (!IsStrictlyWalkable(point) || 
                    !IsStrictlyWalkable(point + perpendicular) || 
                    !IsStrictlyWalkable(point - perpendicular))
                {
                    return false;
                }
            }

            return true;
        }

        private Vector2Int WorldToGridPosition(Vector2 worldPosition)
        {
            return new Vector2Int(
                Mathf.RoundToInt(worldPosition.x / gridSpacing),
                Mathf.RoundToInt(worldPosition.y / gridSpacing)
            );
        }

        public Vector2? GetNearestWalkablePosition(Vector2 position)
        {
            Vector2Int gridPos = WorldToGridPosition(position);
            float minDistance = float.MaxValue;
            NavigationNode nearestNode = null;

            // Busca em área para encontrar o nó mais próximo
            for (int x = -5; x <= 5; x++)
            {
                for (int y = -5; y <= 5; y++)
                {
                    Vector2Int searchPos = new Vector2Int(gridPos.x + x, gridPos.y + y);
                    if (navigationGrid.TryGetValue(searchPos, out NavigationNode node))
                    {
                        float distance = Vector2.Distance(position, node.WorldPosition);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestNode = node;
                        }
                    }
                }
            }

            return nearestNode?.WorldPosition;
        }

        private List<Vector2> ReconstructPath(NavigationNode endNode)
        {
            var path = new List<Vector2>();
            var current = endNode;

            while (current != null)
            {
                path.Add(current.WorldPosition);
                current = current.Parent;
            }

            path.Reverse();
            return OptimizePath(path);
        }

        private List<Vector2> OptimizePath(List<Vector2> path)
        {
            if (path.Count <= 2)
                return path;

            var optimizedPath = new List<Vector2> { path[0] };
            int current = 0;

            while (current < path.Count - 1)
            {
                int furthest = current + 1;
                
                for (int check = current + 2; check < path.Count; check++)
                {
                    if (IsPathSafe(path[current], path[check]))
                    {
                        furthest = check;
                    }
                }

                optimizedPath.Add(path[furthest]);
                current = furthest;
            }

            return optimizedPath;
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGrid) return;

            // Draw map bounds if available
            if (showBounds && walkableArea != null)
            {
                var collider = walkableArea.GetComponent<Collider2D>();
                if (collider != null)
                {
                    Gizmos.color = boundsColor;
                    Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
                }
            }

            if (navigationGrid == null) return;

            float adjustedNodeSize = nodeSize * mapScaleFactor;
            float adjustedConnectionWidth = connectionWidth * mapScaleFactor;

            // Draw nodes and connections
            foreach (var node in navigationGrid.Values)
            {
                if (showNodes)
                {
                    Gizmos.color = nodeColor;
                    Gizmos.DrawSphere((Vector3)node.WorldPosition, adjustedNodeSize);
                }

                if (showConnections)
                {
                    Gizmos.color = connectionColor;
                    foreach (var connection in node.Connections)
                    {
                        // Convert Vector2 to Vector3 for calculations
                        Vector3 startPos = (Vector3)node.WorldPosition;
                        Vector3 endPos = (Vector3)connection.WorldPosition;
                        Vector3 direction = endPos - startPos;
                        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized * adjustedConnectionWidth;
                        
                        Gizmos.DrawLine(
                            startPos + perpendicular,
                            endPos + perpendicular
                        );
                        Gizmos.DrawLine(
                            startPos - perpendicular,
                            endPos - perpendicular
                        );
                    }
                }
            }

            // Draw current path if available
            if (currentDebugPath != null && currentDebugPath.Count > 0)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < currentDebugPath.Count - 1; i++)
                {
                    Gizmos.DrawLine((Vector3)currentDebugPath[i], (Vector3)currentDebugPath[i + 1]);
                    Gizmos.DrawWireSphere((Vector3)currentDebugPath[i], adjustedNodeSize * 1.5f);
                }
                Gizmos.DrawWireSphere((Vector3)currentDebugPath[currentDebugPath.Count - 1], adjustedNodeSize * 1.5f);
            }
        }

        private void SaveGridData()
        {
            #if UNITY_EDITOR
            if (gridData == null)
            {
                gridData = ScriptableObject.CreateInstance<GridData>();
                UnityEditor.AssetDatabase.CreateAsset(gridData, "Assets/Data/GridData.asset");
            }

            gridData.gridSpacing = gridSpacing;
            gridData.mapBounds = mapBounds;
            gridData.nodes.Clear();

            foreach (var node in navigationGrid.Values)
            {
                var serializableNode = new GridData.SerializableNode(node.WorldPosition);
                
                // Store connection indices
                foreach (var connection in node.Connections)
                {
                    int index = navigationGrid.Values.ToList().IndexOf(connection);
                    serializableNode.connectionIndices.Add(index);
                }
                
                gridData.nodes.Add(serializableNode);
            }

            UnityEditor.EditorUtility.SetDirty(gridData);
            UnityEditor.AssetDatabase.SaveAssets();
            #endif
        }

        private void LoadGridData()
        {
            navigationGrid = new Dictionary<Vector2Int, NavigationNode>();
            var nodes = new List<NavigationNode>();

            // Create nodes
            foreach (var nodeData in gridData.nodes)
            {
                var node = new NavigationNode(nodeData.position);
                nodes.Add(node);
                navigationGrid[WorldToGridPosition(nodeData.position)] = node;
            }

            // Connect nodes
            for (int i = 0; i < gridData.nodes.Count; i++)
            {
                var nodeData = gridData.nodes[i];
                var node = nodes[i];

                foreach (int connectionIndex in nodeData.connectionIndices)
                {
                    node.AddConnection(nodes[connectionIndex]);
                }
            }
        }

        // Método público para controlar a visibilidade do caminho
        public void SetPathVisibility(bool visible)
        {
            showPathLine = visible;
            if (pathVisualizer != null)
            {
                pathVisualizer.enabled = visible;
                if (!visible)
                {
                    pathVisualizer.positionCount = 0;
                }
            }
        }

        private void CalculateMapScale()
        {
            if (!autoAdjustSize || walkableArea == null) return;

            var bounds = walkableArea.GetComponent<Collider2D>().bounds;
            float mapSize = Mathf.Max(bounds.size.x, bounds.size.y);
            mapScaleFactor = (referenceMapSize / mapSize) * sizeMultiplier;
        }
    }
}
