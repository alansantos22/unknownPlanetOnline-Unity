using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

namespace UnknownPlanet
{
    public class ConstructionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject constructionUIPrefab;
        [SerializeField] private Canvas gameCanvas;
        [SerializeField] private Transform constructionParent;

        [Header("Map References")]
        [SerializeField] private SpriteRenderer biomeMapRenderer;
        [SerializeField] private SpriteRenderer waterMapRenderer;
        [SerializeField] private LayerMask mapLayer = 1 << 8; // Defina para layer "Bioma" (geralmente é 8)

        [Header("Debug")]
        [SerializeField] private List<Construction> constructions = new List<Construction>();

        [Header("Map Generator Reference")]
        [SerializeField] private MapGenerator mapGenerator;

        private GameObject activeUI;

        void Start()
        {
            // Check for required components
            if (constructionUIPrefab == null)
            {
                Debug.LogError("Construction UI Prefab is not assigned!");
                enabled = false;
                return;
            }

            if (gameCanvas == null)
            {
                // Try to find canvas in scene
                gameCanvas = FindObjectOfType<Canvas>();
                if (gameCanvas == null)
                {
                    Debug.LogError("Game Canvas is not assigned and couldn't be found in scene!");
                    enabled = false;
                    return;
                }
            }

            if (mapGenerator == null)
            {
                mapGenerator = FindObjectOfType<MapGenerator>();
                if (mapGenerator == null)
                {
                    Debug.LogError("MapGenerator not found!");
                    enabled = false;
                    return;
                }
            }

            // Configure Canvas settings with higher sorting order
            gameCanvas.sortingOrder = 10;
            gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            SetupConstructionUIPrefab();

            if (constructionParent == null)
            {
                var container = new GameObject("ConstructionsContainer");
                constructionParent = container.transform;
            }

            // Add collider to biomeMapRenderer if it doesn't exist
            if (biomeMapRenderer != null && biomeMapRenderer.GetComponent<Collider2D>() == null)
            {
                Debug.Log("Adding BoxCollider2D to biomeMapRenderer");
                biomeMapRenderer.gameObject.AddComponent<BoxCollider2D>();
            }

            // Set the correct layer
            if (biomeMapRenderer != null)
            {
                // Get the first (and only) set bit from the layer mask
                int layerNumber = 0;
                for (int i = 0; i < 32; i++)
                {
                    if ((mapLayer.value & (1 << i)) != 0)
                    {
                        layerNumber = i;
                        break;
                    }
                }

                Debug.Log($"Setting biomeMapRenderer to layer {layerNumber}");
                biomeMapRenderer.gameObject.layer = layerNumber;
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                HandleMapClick();
            }
        }

        private void SetupConstructionUIPrefab()
        {
            if (constructionUIPrefab != null)
            {
                // Garantir que o Canvas está configurado corretamente
                var canvas = constructionUIPrefab.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = constructionUIPrefab.AddComponent<Canvas>();
                }
                canvas.overrideSorting = true;
                canvas.sortingOrder = 11; // Maior que o gameCanvas

                // Garantir raycaster para interações
                var raycaster = constructionUIPrefab.GetComponent<GraphicRaycaster>();
                if (raycaster == null)
                {
                    raycaster = constructionUIPrefab.AddComponent<GraphicRaycaster>();
                }

                // Configurar CanvasGroup
                var canvasGroup = constructionUIPrefab.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = constructionUIPrefab.AddComponent<CanvasGroup>();
                }
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;

                // Configurar background para bloquear raycasts
                var panel = constructionUIPrefab.GetComponent<Image>();
                if (panel == null)
                {
                    panel = constructionUIPrefab.AddComponent<Image>();
                    panel.color = new Color(0, 0, 0, 0.5f);
                }
                panel.raycastTarget = true;
            }
        }

        private Vector3 mousePos;

        private void HandleMapClick()
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            Debug.Log($"Mouse clicked at world position: {mousePos2D}");

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            
            if (hit.collider != null)
            {
                Vector2 mapPosition = hit.point;
                Vector2Int gridPosition = new Vector2Int(
                    Mathf.FloorToInt(mapPosition.x),
                    Mathf.FloorToInt(mapPosition.y)
                );

                BiomeType biome = DetermineBiomeFromMousePosition();
                bool isWater = biome == BiomeType.Ocean;

                Debug.Log($"Clicked on biome: {biome}");
                Debug.Log($"Terrain type: {(isWater ? "Water" : "Land")}");

                OpenConstructionUI(gridPosition);
            }
            else
            {
                Debug.LogWarning($"No hit detected! Layer mask: {mapLayer.value}");
                Debug.LogWarning($"BiomeMap layer: {biomeMapRenderer?.gameObject.layer}");
            }
        }

        private BiomeType DetermineBiomeFromMousePosition()
        {
            if (biomeMapRenderer == null || biomeMapRenderer.sprite == null)
            {
                Debug.LogError("BiomeMapRenderer or sprite is null!");
                return BiomeType.Plains;
            }

            // Get mouse position in world space
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = biomeMapRenderer.transform.position.z;

            // Convert to local coordinates
            Vector2 localPoint = biomeMapRenderer.transform.InverseTransformPoint(mousePos);

            // Get texture coordinates (0-1)
            Vector2 normalizedCoord = new Vector2(
                (localPoint.x / biomeMapRenderer.bounds.size.x + 0.5f),
                (localPoint.y / biomeMapRenderer.bounds.size.y + 0.5f)
            );

            // Convert to pixel coordinates
            Vector2Int pixelCoord = new Vector2Int(
                Mathf.RoundToInt(normalizedCoord.x * biomeMapRenderer.sprite.texture.width),
                Mathf.RoundToInt(normalizedCoord.y * biomeMapRenderer.sprite.texture.height)
            );

            // Sample color at position
            Color color = biomeMapRenderer.sprite.texture.GetPixel(
                Mathf.Clamp(pixelCoord.x, 0, biomeMapRenderer.sprite.texture.width - 1),
                Mathf.Clamp(pixelCoord.y, 0, biomeMapRenderer.sprite.texture.height - 1)
            );

            Debug.Log($"Mouse World Pos: {mousePos}");
            Debug.Log($"Local Point: {localPoint}");
            Debug.Log($"Normalized Coord: {normalizedCoord}");
            Debug.Log($"Pixel Coord: {pixelCoord}");
            Debug.Log($"Sampled Color: R:{color.r:F3} G:{color.g:F3} B:{color.b:F3}");

            return DetermineBiomeTypeFromColor(color);
        }

        private BiomeType DetermineBiomeTypeFromColor(Color color)
        {
            Debug.Log($"Analyzing color: R:{color.r:F3} G:{color.g:F3} B:{color.b:F3}");
            
            if (color.b > 0.6f && color.r < 0.2f && color.g < 0.2f)
            {
                Debug.Log("Detected: Ocean");
                return BiomeType.Ocean;
            }
            if (color.r > 0.9f && color.g > 0.9f && color.b > 0.9f)
            {
                Debug.Log("Detected: Snow");
                return BiomeType.Snow;
            }
            if (color.g > 0.2f && color.g < 0.4f && color.r < 0.1f && color.b < 0.1f)
            {
                Debug.Log("Detected: Dense Forest");
                return BiomeType.DenseForest;
            }
            if (color.g > 0.4f && color.g < 0.6f && color.r < 0.1f && color.b < 0.1f)
            {
                Debug.Log("Detected: Forest");
                return BiomeType.Forest;
            }
            if (ColorApprox(color, new Color(0.76f, 0.70f, 0.50f), 0.1f))
            {
                Debug.Log("Detected: Desert");
                return BiomeType.Desert;
            }
            if (ColorApprox(color, new Color(0.7f, 0.8f, 0.3f), 0.1f))
            {
                Debug.Log("Detected: Cerrado");
                return BiomeType.Cerrado;
            }
            if (ColorApprox(color, Color.gray, 0.2f))
            {
                Debug.Log("Detected: Mountain");
                return BiomeType.Mountain;
            }
            
            Debug.Log("Defaulting to: Plains");
            return BiomeType.Plains;
        }

        private Color GetBiomeColorAtPosition(Vector2 worldPos)
        {
            // Converter posição do mundo para coordenadas de textura
            Vector2 localPoint = biomeMapRenderer.transform.InverseTransformPoint(worldPos);
            Vector2 texCoord = new Vector2(
                (localPoint.x / biomeMapRenderer.bounds.size.x + 0.5f) * biomeMapRenderer.sprite.texture.width,
                (localPoint.y / biomeMapRenderer.bounds.size.y + 0.5f) * biomeMapRenderer.sprite.texture.height
            );

            return biomeMapRenderer.sprite.texture.GetPixel(
                Mathf.FloorToInt(texCoord.x),
                Mathf.FloorToInt(texCoord.y)
            );
        }

        private void OpenConstructionUI(Vector2Int coordinates)
        {
            Debug.Log("Attempting to open Construction UI");
            
            if (gameCanvas == null)
            {
                Debug.LogError("Canvas is null!");
                return;
            }

            if (constructionUIPrefab == null)
            {
                Debug.LogError("UI Prefab is null!");
                return;
            }

            if (activeUI != null)
            {
                Debug.Log("Destroying previous UI");
                Destroy(activeUI);
            }

            Debug.Log("Instantiating new UI");
            activeUI = Instantiate(constructionUIPrefab, gameCanvas.transform);
            var ui = activeUI.GetComponentInChildren<ConstructionUI>();
            
            if (ui != null)
            {
                Debug.Log("Found UI component, initializing");
                ui.Initialize(this, coordinates);
            }
            else
            {
                Debug.LogError("No ConstructionUI component found in prefab hierarchy!");
                Destroy(activeUI);
            }
        }

        public void CreateConstruction(string name, BuildingType type, Vector2Int coordinates)
        {
            if (constructions.Exists(c => c.coordinates == coordinates))
            {
                Debug.LogWarning($"Construction already exists at coordinates {coordinates}");
                return;
            }

            if (!CanBuildAt(type, coordinates))
            {
                Debug.LogWarning($"Cannot build {type} here - too close to another {type}");
                return;
            }

            var construction = new Construction
            {
                name = name,
                type = type,
                coordinates = coordinates
            };

            // Create visual representation under the parent
            if (construction.visualPrefab != null)
            {
                var visual = Instantiate(construction.visualPrefab, constructionParent);
                visual.transform.position = new Vector3(coordinates.x, coordinates.y, -1);
            }

            constructions.Add(construction);

            if (activeUI != null)
            {
                Destroy(activeUI);
            }
        }

        public Construction GetConstructionAt(Vector2Int coordinates)
        {
            return constructions.Find(c => c.coordinates == coordinates);
        }

        public bool CanBuildAt(BuildingType type, Vector2Int coordinates)
        {
            var data = type.GetData();
            
            // Verificar bioma
            Color biomeColor = GetBiomeColorAtPosition(new Vector2(coordinates.x, coordinates.y));
            BiomeType biome = DetermineBiomeFromColor(biomeColor);
            
            if (!data.allowedBiomes.Contains(biome))
            {
                Debug.LogWarning($"Cannot build {type} on {biome} biome");
                return false;
            }

            // Verificar construções próximas usando distância euclidiana
            foreach (var construction in constructions)
            {
                if (construction.type == type)
                {
                    float distance = Vector2.Distance(
                        new Vector2(coordinates.x, coordinates.y),
                        new Vector2(construction.coordinates.x, construction.coordinates.y)
                    );
                    
                    if (distance <= data.exclusionRange)
                    {
                        Debug.LogWarning($"Too close to another {type} (distance: {distance:F2})");
                        return false;
                    }
                }
            }

            return true;
        }

        private BiomeType DetermineBiomeFromColor(Color color)
        {
            if (mapGenerator != null)
            {
                // Adicionar logs para debug
                Debug.Log($"Mouse Position (World): {mousePos}");
                
                // Converter para coordenadas locais do sprite
                Vector2 localPoint = biomeMapRenderer.transform.InverseTransformPoint(mousePos);
                Debug.Log($"Local Point (Sprite Space): {localPoint}");
                
                // Normalizar coordenadas (0-1)
                Vector2 normalizedCoord = new Vector2(
                    (localPoint.x / biomeMapRenderer.bounds.size.x + 0.5f),
                    (localPoint.y / biomeMapRenderer.bounds.size.y + 0.5f)
                );
                Debug.Log($"Normalized Coordinates (0-1): {normalizedCoord}");
                
                // Converter para coordenadas do mapa
                int mapX = Mathf.RoundToInt(normalizedCoord.x * mapGenerator.width);
                int mapY = Mathf.RoundToInt(normalizedCoord.y * mapGenerator.height);
                
                // Garantir que as coordenadas estão dentro dos limites
                mapX = Mathf.Clamp(mapX, 0, mapGenerator.width - 1);
                mapY = Mathf.Clamp(mapY, 0, mapGenerator.height - 1);
                
                Debug.Log($"Map Coordinates (Grid): ({mapX}, {mapY})");
                Debug.Log($"Color at position: R:{color.r:F3} G:{color.g:F3} B:{color.b:F3}");

                BiomeType detectedBiome = mapGenerator.GetBiomeAt(mapX, mapY);
                Debug.Log($"Detected Biome from MapGenerator: {detectedBiome}");
                return detectedBiome;
            }

            // Fallback para detecção por cor se não tiver MapGenerator
            Debug.Log($"Using color fallback detection. Color: R:{color.r:F3} G:{color.g:F3} B:{color.b:F3}");

            if (color.b > 0.6f && color.r < 0.2f && color.g < 0.2f) 
            {
                Debug.Log("Detected: Ocean");
                return BiomeType.Ocean;
            }
            if (color.r > 0.9f && color.g > 0.9f && color.b > 0.9f)
            {
                Debug.Log("Detected: Snow");
                return BiomeType.Snow;
            }
            if (color.g > 0.2f && color.g < 0.4f && color.r < 0.1f && color.b < 0.1f)
            {
                Debug.Log("Detected: Dense Forest");
                return BiomeType.DenseForest;
            }
            if (color.g > 0.4f && color.g < 0.6f && color.r < 0.1f && color.b < 0.1f)
            {
                Debug.Log("Detected: Forest");
                return BiomeType.Forest;
            }
            if (ColorApprox(color, new Color(0.76f, 0.70f, 0.50f), 0.1f))
            {
                Debug.Log("Detected: Desert");
                return BiomeType.Desert;
            }
            if (ColorApprox(color, new Color(0.7f, 0.8f, 0.3f), 0.1f))
            {
                Debug.Log("Detected: Cerrado");
                return BiomeType.Cerrado;
            }
            if (ColorApprox(color, Color.gray, 0.2f))
            {
                Debug.Log("Detected: Mountain");
                return BiomeType.Mountain;
            }
            
            Debug.Log("Defaulting to: Plains");
            return BiomeType.Plains;
        }

        private bool ColorApprox(Color a, Color b, float tolerance = 0.1f)
        {
            return Mathf.Abs(a.r - b.r) < tolerance &&
                   Mathf.Abs(a.g - b.g) < tolerance &&
                   Mathf.Abs(a.b - b.b) < tolerance;
        }
    }
}