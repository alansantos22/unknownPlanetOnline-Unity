using UnityEngine;
using UnknownPlanet;
using System.Collections;  // Add this line

namespace UnknownPlanet
{
    public class HexMapGenerator : MonoBehaviour
    {
        [Header("Map Settings")]
        public int width = 10;
        public int height = 10;
        public float hexSize = 1f;

        [Header("References")]
        public GameObject hexPrefab;
        public MapGenerator mapGenerator;
        public Transform mapOrigin; // Novo: ponto de referência para início do mapa

        [Header("Debug")]
        public bool showGizmos = true;
        public Color gizmoColor = Color.yellow;

        [Header("Grid Settings")]
        public float defaultHexScale = 0.5f;
        private float currentHexScale;
        private Hex[,] hexGrid;

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Vector3 origin = mapOrigin != null ? mapOrigin.position : transform.position;
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(origin, 0.5f);

            // Desenha o contorno da área do mapa
            Vector3 topRight = CalculateHexPosition(width - 1, height - 1, origin);
            Gizmos.DrawWireCube(
                (origin + topRight) * 0.5f,
                new Vector3(width * hexSize * 0.75f, height * hexSize * Mathf.Sqrt(3) / 2, 0)
            );
        }

        private Vector3 CalculateHexPosition(int x, int y, Vector3 origin)
        {
            float xPos = x * hexSize * 0.75f;
            float yPos = y * hexSize * Mathf.Sqrt(3) / 2 + (x % 2 == 0 ? 0 : hexSize * Mathf.Sqrt(3) / 4);
            return origin + new Vector3(xPos, yPos, -2);
        }

        void Awake()
        {
            // Ensure MapGenerator is ready before we start
            mapGenerator = FindObjectOfType<MapGenerator>();
            if (mapGenerator == null)
            {
                Debug.LogError("MapGenerator not found in scene!");
                enabled = false;
                return;
            }
        }

        void Start()
        {
            // Wait a frame to ensure MapGenerator has initialized
            StartCoroutine(DelayedStart());
        }

        private IEnumerator DelayedStart()
        {
            yield return new WaitForEndOfFrame();

            if (!enabled) yield break;

            if (hexPrefab == null)
            {
                Debug.LogError("HexPrefab reference is not set.");
                yield break;
            }

            if (mapGenerator.noiseMap == null)
            {
                Debug.LogError("NoiseMap is not generated.");
                yield break;
            }

            GenerateHexMap();
        }

        void GenerateHexMap()
        {
            Vector3 origin = mapOrigin != null ? mapOrigin.position : transform.position;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 position = CalculateHexPosition(x, y, origin);
                    GameObject hex = Instantiate(hexPrefab, position, Quaternion.identity, transform);

                    var hexComponent = hex.GetComponent<Hex>();
                    hexComponent.coordinates = new Vector2Int(x, y);
                    
                    // Calcular altura e latitude normalizadas
                    float height = mapGenerator.noiseMap[x, y];
                    float latitude = Mathf.Abs((float)y / this.height - 0.5f) * 2f;
                    
                    hexComponent.Initialize(height, latitude); // Mudado para usar o novo método Initialize
                }
            }
        }

        public void GenerateHexGrid(int gridWidth, int gridHeight, float hexScale)
        {
            if (hexPrefab == null || mapGenerator == null) return;

            currentHexScale = hexScale;
            hexGrid = new Hex[gridWidth, gridHeight];
            
            // Limpar grid existente
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            Vector3 startPos = transform.position;
            float hexWidth = hexScale * 1.732f;
            float hexHeight = hexScale * 2f;
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    // Posição do hexágono no mundo
                    float xPos = startPos.x + x * (hexWidth * 0.75f);
                    float yPos = startPos.y + y * hexHeight;
                    if (x % 2 == 1) yPos += hexHeight * 0.5f;

                    Vector3 hexPos = new Vector3(xPos, yPos, 0);
                    GameObject hexObj = Instantiate(hexPrefab, hexPos, Quaternion.identity, transform);
                    hexObj.transform.localScale = Vector3.one * hexScale;
                    
                    Hex hex = hexObj.GetComponent<Hex>();
                    hexGrid[x, y] = hex;
                    hex.coordinates = new Vector2Int(x, y);

                    // Calcular altura e latitude normalizadas
                    float height = mapGenerator.noiseMap[x, y];
                    float latitude = Mathf.Abs((float)y / gridHeight - 0.5f) * 2f; // 0 no equador, 1 nos polos
                    
                    hex.Initialize(height, latitude); // Mudado para usar o novo método Initialize
                }
            }
        }

        public Hex GetHexAt(Vector2Int coordinates)
        {
            if (hexGrid == null) return null;
            if (coordinates.x < 0 || coordinates.x >= hexGrid.GetLength(0)) return null;
            if (coordinates.y < 0 || coordinates.y >= hexGrid.GetLength(1)) return null;
            
            return hexGrid[coordinates.x, coordinates.y];
        }
    }
}