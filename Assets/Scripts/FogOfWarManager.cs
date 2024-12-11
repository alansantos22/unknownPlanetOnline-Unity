using UnityEngine;
using System.Collections.Generic;

// Classe para armazenar dados de cidades descobertas
[System.Serializable]
public class CityData
{
    public Vector2 position;
    public float revealRadius;
    public bool isDiscovered;
}

public class FogOfWarManager : MonoBehaviour
{
    [Header("Configurações do Fog of War")]
    [Tooltip("Material que usa o shader FogOfWar2D")]
    [SerializeField] private Material fogMaterial;
    
    [Tooltip("Tamanho da textura do fog (maior = mais detalhado, mas mais pesado)")]
    // Remover textureSize pois usaremos as dimensões do sprite
    
    [Header("Configurações de Revelação")]
    
    [SerializeField] private float explorationRadius = 5f;
    
    [Tooltip("Raio de visão do player")]
    [SerializeField] private float playerRevealRadius = 0.25f;
    
    [Tooltip("Raio de revelação ao descobrir uma cidade")]
    [SerializeField] private float cityRevealRadius = 1f;
    
    [Tooltip("Transform do player para acompanhamento automático")]
    [SerializeField] private Transform playerTransform;
    
    [Header("Referencias")]
    [SerializeField] private SpriteRenderer fogRenderer; // Adicione esta referência para o seu sprite

    private List<CityData> cities = new List<CityData>();
    
    private Texture2D exploredAreasTexture;
    private Color[] clearColors;
    private Color[] fogColors;
    private Vector2 debugPlayerPos; // Add this field to store player position for debug visualization

    void Start()
    {
        if (fogRenderer == null)
        {
            fogRenderer = GetComponent<SpriteRenderer>();
            if (fogRenderer == null)
            {
                Debug.LogError("Fog Renderer não encontrado!");
                enabled = false;
                return;
            }
        }

        InitializeTextures();
    }

    void LateUpdate()
    {
        if (playerTransform != null)
        {
            Vector2 playerPos = playerTransform.position;
            RevealArea(playerPos, playerRevealRadius, false);
            debugPlayerPos = playerPos; // Store for debug visualization
        }
    }

    void OnDrawGizmos()
    {
        if (enabled && playerTransform != null && Debug.isDebugBuild)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(debugPlayerPos, playerRevealRadius);
        }
    }

    public void Initialize(Transform player)
    {
        if (player == null)
        {
            Debug.LogError("Tentativa de inicializar FogOfWar com player nulo!");
            return;
        }

        playerTransform = player;
        RevealArea(player.position, playerRevealRadius, false);
        enabled = true;
    }

    /// <summary>
    /// Inicializa as texturas necessárias para o fog of war
    /// </summary>
    void InitializeTextures()
    {
        if (fogRenderer == null || fogRenderer.sprite == null)
        {
            Debug.LogError("Fog renderer ou sprite não encontrado!");
            return;
        }

        // Usar as dimensões do sprite
        int width = fogRenderer.sprite.texture.width;
        int height = fogRenderer.sprite.texture.height;
        
        exploredAreasTexture = new Texture2D(width, height);
        clearColors = new Color[width * height];
        fogColors = new Color[width * height];

        for (int i = 0; i < fogColors.Length; i++)
        {
            clearColors[i] = Color.white;
            fogColors[i] = Color.black;
        }

        exploredAreasTexture.SetPixels(fogColors);
        exploredAreasTexture.Apply();

        fogRenderer.material.SetTexture("_ExploredAreas", exploredAreasTexture);
    }

    // Método de conveniência para revelar área com raio padrão
    public void RevealArea(Vector2 worldPosition)
    {
        RevealArea(worldPosition, explorationRadius, false);
    }

    /// <summary>
    /// Revela uma área no fog of war
    /// </summary>
    /// <param name="worldPosition">Posição no mundo a ser revelada</param>
    /// <param name="radius">Raio da área a ser revelada</param>
    /// <param name="permanent">Se verdadeiro, a área ficará permanentemente revelada</param>
    public void RevealArea(Vector2 worldPosition, float radius, bool permanent)
    {
        if (fogRenderer == null || fogRenderer.sprite == null) return;

        Vector2 texturePos;
        float scaleX, scaleY;
        WorldToTexturePosition(worldPosition, out texturePos, out scaleX, out scaleY);

        float radiusInPixelsX = radius * scaleX;
        float radiusInPixelsY = radius * scaleY;
        int intRadius = Mathf.CeilToInt(Mathf.Max(radiusInPixelsX, radiusInPixelsY));

        // Se não for permanente, aplicar um efeito de fade gradual
        float fadeMultiplier = permanent ? 1f : 0.5f;
        Color revealColor = permanent ? Color.white : new Color(0.5f, 0.5f, 0.5f, fadeMultiplier);

        for (int x = -intRadius; x <= intRadius; x++)
        {
            for (int y = -intRadius; y <= intRadius; y++)
            {
                float adjustedX = x / (radiusInPixelsX / radiusInPixelsY);
                if (adjustedX * adjustedX + y * y <= intRadius * intRadius)
                {
                    int pixelX = Mathf.RoundToInt(texturePos.x + x);
                    int pixelY = Mathf.RoundToInt(texturePos.y + y);

                    if (pixelX >= 0 && pixelX < fogRenderer.sprite.texture.width && 
                        pixelY >= 0 && pixelY < fogRenderer.sprite.texture.height)
                    {
                        Color currentColor = exploredAreasTexture.GetPixel(pixelX, pixelY);
                        if (permanent || currentColor.r < 0.5f)
                        {
                            exploredAreasTexture.SetPixel(pixelX, pixelY, revealColor);
                        }
                    }
                }
            }
        }

        exploredAreasTexture.Apply();
    }

    /// <summary>
    /// Adiciona uma nova cidade ao sistema
    /// </summary>
    /// <param name="position">Posição da cidade no mundo</param>
    public void AddCity(Vector2 position)
    {
        CityData city = new CityData
        {
            position = position,
            revealRadius = cityRevealRadius,
            isDiscovered = false
        };
        cities.Add(city);
    }

    /// <summary>
    /// Marca uma cidade como descoberta e revela permanentemente sua área
    /// </summary>
    /// <param name="cityPosition">Posição da cidade a ser descoberta</param>
    public void DiscoverCity(Vector2 cityPosition)
    {
        var city = cities.Find(c => Vector2.Distance(c.position, cityPosition) < 0.1f);
        if (city != null && !city.isDiscovered)
        {
            city.isDiscovered = true;
            RevealArea(city.position, city.revealRadius, true);
        }
    }

    /// <summary>
    /// Salva o estado atual das áreas descobertas e cidades
    /// </summary>
    public void SaveDiscoveredAreas()
    {
        byte[] bytes = exploredAreasTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.persistentDataPath + "/fogofwar.png", bytes);
        
        string citiesJson = JsonUtility.ToJson(new SerializableCityList { cities = cities });
        System.IO.File.WriteAllText(Application.persistentDataPath + "/cities.json", citiesJson);
    }

    /// <summary>
    /// Carrega o estado salvo das áreas descobertas e cidades
    /// </summary>
    public void LoadDiscoveredAreas()
    {
        string path = Application.persistentDataPath + "/fogofwar.png";
        if (System.IO.File.Exists(path))
        {
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            exploredAreasTexture.LoadImage(bytes);
            exploredAreasTexture.Apply();
        }

        string citiesPath = Application.persistentDataPath + "/cities.json";
        if (System.IO.File.Exists(citiesPath))
        {
            string json = System.IO.File.ReadAllText(citiesPath);
            SerializableCityList loadedCities = JsonUtility.FromJson<SerializableCityList>(json);
            cities = loadedCities.cities;
        }
    }

    /// <summary>
    /// Converte uma posição do mundo para coordenadas de textura
    /// </summary>
    /// <param name="worldPos">Posição no mundo</param>
    /// <returns>Posição correspondente na textura do fog</returns>
    private void WorldToTexturePosition(Vector2 worldPos, out Vector2 texturePos, out float scaleX, out float scaleY)
    {
        // Obter tamanho real do sprite no mundo
        Vector2 spriteSize = fogRenderer.bounds.size;
        
        // Obter dimensões da textura
        float textureWidth = fogRenderer.sprite.texture.width;
        float textureHeight = fogRenderer.sprite.texture.height;

        // Calcular escalas
        scaleX = textureWidth / spriteSize.x;
        scaleY = textureHeight / spriteSize.y;

        // Obter posição relativa ao sprite do fog
        Vector2 fogOrigin = fogRenderer.transform.position;
        Vector2 relativePos = worldPos - fogOrigin;

        // Converter para coordenadas de textura
        texturePos = new Vector2(
            (relativePos.x + spriteSize.x * 0.5f) * scaleX,
            (relativePos.y + spriteSize.y * 0.5f) * scaleY
        );
    }

    // Add this method near the other public methods
    public float GetCityRevealRadius()
    {
        return cityRevealRadius;
    }
}

[System.Serializable]
public class SerializableCityList
{
    public List<CityData> cities;
}