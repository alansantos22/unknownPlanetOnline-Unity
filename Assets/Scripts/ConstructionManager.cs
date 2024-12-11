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
        [SerializeField] private GameObject player; // Adicione esta linha

        [Header("Map References")]
        [SerializeField] private SpriteRenderer biomeMapRenderer;
        [SerializeField] private SpriteRenderer waterMapRenderer;
        [SerializeField] private LayerMask mapLayer = 1 << 8; // Defina para layer "Bioma" (geralmente é 8)

        [Header("Debug")]
        [SerializeField] private List<Construction> constructions = new List<Construction>();

        [Header("Map Generator Reference")]
        [SerializeField] private MapGenerator mapGenerator;

        [Header("Camera Control")]
        [SerializeField] private CameraController cameraController;  // Adicione esta referência

        [Header("Building Prefabs")]
        [SerializeField] private GameObject cityPrefab;
        [SerializeField] private GameObject villagePrefab;
        [SerializeField] private GameObject farmPrefab;
        [SerializeField] private GameObject minePrefab;

        [SerializeField] private FogOfWarManager fogOfWar;

        private GameObject activeUI;

        void Start()
        {
            // Try to find Player if not assigned
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                {
                    Debug.LogError("Player não encontrado! Certifique-se de que seu player tem a tag 'Player'.");
                    enabled = false; // Desabilitar o ConstructionManager se não encontrar o player
                    return;
                }
                Debug.Log("Player encontrado automaticamente: " + player.name);
            }

            // Try to find FogOfWar if not assigned
            if (fogOfWar == null)
            {
                fogOfWar = FindObjectOfType<FogOfWarManager>();
                if (fogOfWar == null)
                {
                    Debug.LogError("FogOfWar não encontrado na cena!");
                    enabled = false;
                    return;
                }
                Debug.Log("FogOfWar encontrado automaticamente: " + fogOfWar.name);
            }

            // Inicializar FogOfWar com o player encontrado
            fogOfWar.Initialize(player.transform);

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

            if (cameraController == null)
            {
                cameraController = FindObjectOfType<CameraController>();
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

            // Initialize FogOfWar with found player
            if (fogOfWar != null && player != null)
            {
                fogOfWar.Initialize(player.transform);
                Debug.Log($"Initializing FogOfWar with player at {player.transform.position}");
            }
            else
            {
                if (player == null) Debug.LogError("Player reference is missing! Add the 'Player' tag to your player object.");
                if (fogOfWar == null) Debug.LogError("FogOfWar reference is missing! Add FogOfWarManager to your scene.");
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
        private BiomeType currentBiome; // Adicionar esta variável

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

                currentBiome = DetermineBiomeFromMousePosition(); // Armazenar o bioma
                bool isWater = currentBiome == BiomeType.Ocean;

                Debug.Log($"Clicked on biome: {currentBiome}");
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

            if (cameraController != null)
            {
                cameraController.enabled = false;
                cameraController.ResetDragState(); // Adicione esta linha
            }
        }

        public void CloseConstructionUI()
        {
            if (activeUI != null)
            {
                Destroy(activeUI);
            }
            
            if (cameraController != null)
            {
                cameraController.enabled = true;
                cameraController.ResetDragState(); // Adicione esta linha
            }
        }

        private GameObject GetPrefabForType(BuildingType type)
        {
            return type switch
            {
                BuildingType.City => cityPrefab,
                BuildingType.Village => villagePrefab,
                BuildingType.Farm => farmPrefab,
                BuildingType.Mine => minePrefab,
                _ => null
            };
        }

        public void CreateConstruction(string name, BuildingType type, Vector2Int coordinates)
        {
            // Verificações básicas
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("Construction name cannot be empty");
                return;
            }

            if (!CanBuildAt(type, coordinates))
            {
                return; // CanBuildAt já loga o erro
            }

            // Criar a construção
            var construction = new Construction
            {
                name = name,
                type = type,
                coordinates = coordinates,
                visualPrefab = GetPrefabForType(type)
            };

            // Criar representação visual
            if (construction.visualPrefab != null)
            {
                var visual = Instantiate(construction.visualPrefab, constructionParent);
                // Usar a posição do mousePos que foi salva durante o HandleMapClick
                visual.transform.position = new Vector3(mousePos.x, mousePos.y, -1);
                
                // Atualizar as coordenadas da construção para refletir a posição real
                construction.coordinates = new Vector2Int(
                    Mathf.RoundToInt(mousePos.x),
                    Mathf.RoundToInt(mousePos.y)
                );
                
                var identifier = visual.AddComponent<ConstructionIdentifier>();
                identifier.Initialize(construction);

                // Reveal fog of war around new construction
                if (fogOfWar != null)
                {
                    fogOfWar.RevealArea(new Vector2(mousePos.x, mousePos.y), 5f, true);
                }
            }
            else
            {
                Debug.LogWarning($"No prefab found for {type}");
            }

            constructions.Add(construction);
            Debug.Log($"Created {type} named {name} at {coordinates}");

            CloseConstructionUI();
        }

        public Construction GetConstructionAt(Vector2Int coordinates)
        {
            return constructions.Find(c => c.coordinates == coordinates);
        }

        public bool CanBuildAt(BuildingType type, Vector2Int coordinates)
        {
            var data = type.GetData();
            
            Debug.Log($"Attempting to build {type} on {currentBiome}"); // Usar o bioma armazenado
            Debug.Log($"Allowed biomes: {string.Join(", ", data.allowedBiomes)}");

            // Verificar biomas permitidos usando o bioma armazenado
            if (!data.allowedBiomes.Contains(currentBiome))
            {
                Debug.Log($"Cannot build {type} on {currentBiome} biome");
                return false;
            }

            // 3. Se não houver construções, permitir construir
            if (constructions.Count == 0)
            {
                return true;
            }

            // 4. Verificar distância apenas de construções do mesmo tipo
            foreach (var construction in constructions)
            {
                if (construction.type == type)
                {
                    float distance = Vector2.Distance(
                        new Vector2(mousePos.x, mousePos.y), // Usar mousePos em vez de coordinates
                        new Vector2(construction.coordinates.x, construction.coordinates.y)
                    );
                    
                    Debug.Log($"Distance to nearest {type}: {distance}");
                    
                    if (distance < data.exclusionRange)
                    {
                        Debug.Log($"Too close to another {type}");
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
            if (color.r > 0.9f && color.g > 9f && color.b > 0.9f)
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