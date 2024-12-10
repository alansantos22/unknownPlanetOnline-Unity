using UnityEngine;
using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;
using System.Collections.Generic;  // Adicionar esta linha

public class MapGenerator : MonoBehaviour
{
    [Header("Configurações Básicas do Mapa")]
    [Tooltip("Largura do mapa em pixels")]
    [SerializeField]
    private int _width = 100;  // Largura do mapa
    public int width
    {
        get { return _width; }
        set
        {
            _width = value;
            GenerateNoiseMap();
            DrawNoiseMap();
        }
    }

    [Tooltip("Altura do mapa em pixels")]
    [SerializeField]
    private int _height = 100; // Altura do mapa
    public int height
    {
        get { return _height; }
        set
        {
            _height = value;
            GenerateNoiseMap();
            DrawNoiseMap();
        }
    }

    [Tooltip("Escala do ruído para geração do terreno - Valores menores criam terrenos mais suaves")]
    [SerializeField]
    private float _noiseScale = 0.1f; // Escala do ruído para geração do terreno
    public float noiseScale
    {
        get { return _noiseScale; }
        set
        {
            _noiseScale = value;
            GenerateNoiseMap();
            DrawNoiseMap();
        }
    }
    [SerializeField]
    private float _zoom = 100f; // Nível de zoom do mapa
    public float zoom
    {
        get { return _zoom; }
        set
        {
            _zoom = value;
            GenerateNoiseMap();
            DrawNoiseMap();
        }
    }

    [Tooltip("Semente para geração aleatória do mapa - O mesmo valor gera sempre o mesmo mapa")]
    [SerializeField]
    private int _seed = 0; // Semente para geração aleatória consistente
    public int seed
    {
        get { return _seed; }
        set
        {
            _seed = value;
            GenerateNoiseMap();
            DrawNoiseMap();
        }
    }

    [Tooltip("Proporção entre terra e água (0-1) - Valores maiores criam mais terra")]
    [SerializeField]
    private float _landProportion = 0.3f; // Proporção entre terra e água
    public float landProportion
    {
        get { return _landProportion; }
        set
        {
            _landProportion = value;
            DrawNoiseMap();
        }
    }

    [Tooltip("Tamanho de cada pixel no mapa - Valores maiores deixam o mapa mais pixelado")]
    [SerializeField]
    private int _pixelSize = 1; // Tamanho de cada pixel no mapa
    public int pixelSize
    {
        get { return _pixelSize; }
        set
        {
            DrawNoiseMap();
        }
    }

    [Header("Sistema de Temperatura e Umidade")]
    [SerializeField] private float temperatureScale = 0.7f;
    [SerializeField] private float heightInfluence = 0.4f;
    private float[,] temperatureMap;
    private float[,] humidityMap;

    [Header("Water Thresholds")]
    [SerializeField] [Range(0f, 1f)] private float deepWaterThreshold = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float shallowWaterThreshold = 0.6f;

    [Header("Base Colors")]
    public Color landColor = Color.green;      // Cor padrão para terra
    
    [Header("Water Colors")]
    public Color deepOceanColor = new Color(0.0f, 0.0f, 0.4f, 1f);    // Azul escuro
    public Color oceanColor = new Color(0.0f, 0.0f, 0.7f, 1f);        // Azul médio
    public Color shallowWaterColor = new Color(0.0f, 0.4f, 1f, 1f);   // Azul claro
    public Color waterColor = Color.blue;      // Cor padrão para água

    [Header("References")]
    public float[,] noiseMap;                  // Mapa de ruído para altura do terreno
    public SpriteRenderer landRenderer;        // Renderizador para camada de terra
    public SpriteRenderer waterRenderer;       // Renderizador para camada de água
    public Texture2D landTexture;             // Textura para terra
    public Texture2D waterTexture;            // Textura para água

    [Header("Biome Mask")]
    public SpriteRenderer biomeMaskRenderer;

    [System.Serializable]
    public class BiomeSettings
    {
        public string biomeName;
        public Color biomeColor;
        [Range(0f, 1f)] public float distributionRatio = 0.33f; // Proporção deste bioma no total
        [Range(10f, 100f)] public float minSize = 30f;
        [Range(10f, 100f)] public float maxSize = 60f;
        [Range(0f, 1f)] public float temperatureTolerance = 0.5f; // Quanto o bioma tolera temperatura
        [Range(0f, 1f)] public float minLatitude = 0f; // Novo: controle de latitude mínima
        [Range(0f, 1f)] public float maxLatitude = 1f; // Novo: controle de latitude máxima
        [Range(-1f, 1f)] public float preferredTemperature = 0f; // Novo: temperatura ideal para o bioma
    }

    [Header("Biome System")]
    [SerializeField] private BiomeSettings[] biomes;
    [SerializeField] [Range(-1f, 1f)] private float globalTemperature = 0f;
    [SerializeField] private int totalBiomePoints = 30;

    [Header("Biome Generation")]
    [SerializeField] private int cellSize = 32; // Tamanho das células do Voronoi
    [SerializeField] [Range(0f, 1f)] private float voronoiThreshold = 0.5f; // Limite para definir bordas
    [SerializeField] [Range(0f, 1f)] private float mountainHeight = 0.7f;
    [SerializeField] [Range(0f, 1f)] private float snowHeight = 0.85f;
    [SerializeField] [Range(0f, 0.5f)] private float polarRegionSize = 0.25f;

    private class VoronoiPoint
    {
        public Vector2 position;
        public int biomeType;
    }

    private List<VoronoiPoint> voronoiPoints;

    void Awake() // Change Start to Awake to ensure noise map is generated before other scripts access it
    {
        InitializeDefaultBiomes();

        if (landRenderer == null)
        {
            Debug.LogError("[MapGenerator] LandRenderer component missing! Please assign in inspector.");
            return;
        }

        if (waterRenderer == null)
        {
            Debug.LogError("[MapGenerator] WaterRenderer component missing! Please assign in inspector.");
            return;
        }

        try
        {
            GenerateNoiseMap();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MapGenerator] Failed to generate noise map: {e.Message}\n{e.StackTrace}");
            return;
        }

        if (noiseMap == null)
        {
            Debug.LogError("[MapGenerator] NoiseMap generation failed - map is null. Check parameters in inspector.");
            return;
        }

        DrawNoiseMap();
    }

    void Start()
    {
        if (landRenderer == null || waterRenderer == null)
        {
            Debug.LogError("[MapGenerator] Required renderers are missing!");
            return;
        }

        try
        {
            GenerateMap();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MapGenerator] Error generating map: {e.Message}");
        }
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            // Durante a edição, apenas atualizar se todos os componentes estiverem presentes
            if (landRenderer != null && waterRenderer != null && biomeMaskRenderer != null)
            {
                GenerateMap();
            }
        }
        else
        {
            // Durante o gameplay, usar o Invoke para evitar problemas de timing
            if (landRenderer != null && waterRenderer != null && biomeMaskRenderer != null)
            {
                CancelInvoke(nameof(GenerateMap));
                Invoke(nameof(GenerateMap), 0.1f);
            }
        }
    }

    public void SetZoom(float newZoom)
    {
        _zoom = newZoom;
        GenerateMap();
    }

    private void GenerateMap()
    {
        if (width <= 0) width = 100;
        if (height <= 0) height = 100;
        if (_zoom <= 0) _zoom = 100f;
        
        try
        {
            GenerateNoiseMap();
            DrawNoiseMap();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MapGenerator] Error in GenerateMap: {e.Message}");
        }
    }

    void GenerateNoiseMap()
    {
        try {
            if (width <= 0 || height <= 0)
            {
                Debug.LogError("[MapGenerator] Invalid map dimensions");
                return;
            }

            noiseMap = new float[width, height];
            temperatureMap = new float[width, height];
            humidityMap = new float[width, height];
            
            Perlin noise = new Perlin();
            Perlin tempNoise = new Perlin();
            Perlin humidityNoise = new Perlin();
            
            noise.Seed = seed;
            tempNoise.Seed = seed + 1;
            humidityNoise.Seed = seed + 2;

            float minNoise = float.MaxValue;
            float maxNoise = float.MinValue;

            // First pass: generate raw noise
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float sampleX = x * noiseScale / zoom;
                    float sampleY = y * noiseScale / zoom;
                    float noiseValue = (float)noise.GetValue(sampleX, sampleY, 0);
                    
                    // Track min and max for normalization
                    minNoise = Mathf.Min(minNoise, noiseValue);
                    maxNoise = Mathf.Max(maxNoise, noiseValue);
                    
                    noiseMap[x, y] = noiseValue;

                    // Generate temperature and humidity as before
                    float tempX = x * temperatureScale / zoom;
                    float tempY = y * temperatureScale / zoom;
                    float heightTemp = noiseValue * heightInfluence;
                    float baseTemp = (float)tempNoise.GetValue(tempX, tempY, 0);
                    temperatureMap[x, y] = baseTemp - heightTemp;

                    float humidityX = x * temperatureScale / zoom;
                    float humidityY = y * temperatureScale / zoom;
                    float humidityValue = (float)humidityNoise.GetValue(humidityX, humidityY, 0);
                    float latitudeInfluence = Mathf.Abs(y - height/2f) / (height/2f);
                    humidityValue = Mathf.Lerp(humidityValue, -0.5f, latitudeInfluence);
                    humidityMap[x, y] = humidityValue;
                }
            }

            // Second pass: normalize values
            float noiseRange = maxNoise - minNoise;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Normalize to 0-1 range
                    noiseMap[x, y] = (noiseMap[x, y] - minNoise) / noiseRange;
                }
            }
        } catch (System.Exception e) {
            Debug.LogError($"[MapGenerator] Error in GenerateNoiseMap: {e.Message}");
        }
    }

    private void GenerateBiomeMask()
    {
        if (biomeMaskRenderer == null) return;

        try {
            // Inicializar biomas se ainda não foram inicializados
            if (voronoiPoints == null)  // Alterado de biomePoints para voronoiPoints
            {
                InitializeBiomes();
            }

            Texture2D biomeMask = new Texture2D(width * pixelSize, height * pixelSize, TextureFormat.RGBA32, false);
            biomeMask.filterMode = FilterMode.Point;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Defina pixels de água como totalmente transparentes
                    if (noiseMap[x, y] <= landProportion) {
                        FillTextureRegion(biomeMask, x * pixelSize, y * pixelSize, pixelSize, new Color(0, 0, 0, 0));
                        continue;
                    }

                    float normalizedHeight = (noiseMap[x, y] - landProportion) / (1 - landProportion);
                    float latitude = Mathf.Abs(y - height/2f) / (height/2f);
                    float humidity = humidityMap[x, y];
                    
                    Color biomeColor = GetBiomeColor(normalizedHeight, latitude, humidity, x, y);
                    FillTextureRegion(biomeMask, x * pixelSize, y * pixelSize, pixelSize, biomeColor);
                }
            }

            biomeMask.Apply();
            
            if (biomeMaskRenderer.sprite != null)
                DestroyImmediate(biomeMaskRenderer.sprite.texture);
            
            biomeMaskRenderer.sprite = Sprite.Create(
                biomeMask, 
                new Rect(0, 0, biomeMask.width, biomeMask.height), 
                new Vector2(0.5f, 0.5f)
            );

        } catch (System.Exception e) {
            Debug.LogError($"[MapGenerator] Error in GenerateBiomeMask: {e.Message}");
        }
    }

    private void InitializeBiomes()
    {
        // Garantir que os biomas são inicializados primeiro
        InitializeDefaultBiomes();

        System.Random prng = new System.Random(seed);
        voronoiPoints = new List<VoronoiPoint>();

        // Normalizar as distributionRatios
        float totalRatio = 0f;
        foreach (var biome in biomes)
            totalRatio += biome.distributionRatio;

        // Criar pontos Voronoi para cada bioma
        for (int biomeIndex = 0; biomeIndex < biomes.Length; biomeIndex++)
        {
            float normalizedRatio = biomes[biomeIndex].distributionRatio / totalRatio;
            int pointCount = Mathf.Max(1, Mathf.RoundToInt(totalBiomePoints * normalizedRatio));
            
            for (int i = 0; i < pointCount; i++)
            {
                int minY = Mathf.Max(0, (int)(height * biomes[biomeIndex].minLatitude));
                int maxY = Mathf.Min(height, (int)(height * biomes[biomeIndex].maxLatitude));

                voronoiPoints.Add(new VoronoiPoint
                {
                    position = new Vector2(
                        prng.Next(0, width),
                        prng.Next(minY, maxY)
                    ),
                    biomeType = biomeIndex
                });
            }
        }
    }

    private void InitializeDefaultBiomes()
    {
        if (biomes == null || biomes.Length == 0)
        {
            biomes = new BiomeSettings[]
            {
                new BiomeSettings
                {
                    biomeName = "Snow",
                    biomeColor = Color.white,
                    distributionRatio = 0.15f,
                    minSize = 40f,
                    maxSize = 80f,
                    preferredTemperature = -0.8f,
                    temperatureTolerance = 0.3f,
                    minLatitude = 0.7f,
                    maxLatitude = 1f
                },
                new BiomeSettings
                {
                    biomeName = "Tundra",
                    biomeColor = Color.Lerp(Color.white, new Color(0.0f, 0.5f, 0.0f), 0.3f),
                    distributionRatio = 0.1f,
                    minSize = 30f,
                    maxSize = 60f,
                    preferredTemperature = -0.5f,
                    temperatureTolerance = 0.3f,
                    minLatitude = 0.6f,
                    maxLatitude = 0.8f
                },
                new BiomeSettings
                {
                    biomeName = "Dense Forest",
                    biomeColor = new Color(0.0f, 0.3f, 0.0f),
                    distributionRatio = 0.2f,
                    minSize = 50f,
                    maxSize = 90f,
                    preferredTemperature = 0.2f,
                    temperatureTolerance = 0.4f,
                    minLatitude = 0.3f,
                    maxLatitude = 0.7f
                },
                new BiomeSettings
                {
                    biomeName = "Forest",
                    biomeColor = new Color(0.0f, 0.5f, 0.0f),
                    distributionRatio = 0.2f,
                    minSize = 40f,
                    maxSize = 70f,
                    preferredTemperature = 0.0f,
                    temperatureTolerance = 0.5f,
                    minLatitude = 0.2f,
                    maxLatitude = 0.6f
                },
                new BiomeSettings
                {
                    biomeName = "Cerrado",
                    biomeColor = new Color(0.7f, 0.8f, 0.3f),
                    distributionRatio = 0.15f,
                    minSize = 30f,
                    maxSize = 60f,
                    preferredTemperature = 0.4f,
                    temperatureTolerance = 0.3f,
                    minLatitude = 0.2f,
                    maxLatitude = 0.5f
                },
                new BiomeSettings
                {
                    biomeName = "Desert",
                    biomeColor = new Color(0.76f, 0.70f, 0.50f),
                    distributionRatio = 0.2f,
                    minSize = 45f,
                    maxSize = 85f,
                    preferredTemperature = 0.8f,
                    temperatureTolerance = 0.4f,
                    minLatitude = 0.1f,
                    maxLatitude = 0.4f
                }
            };
        }
    }

    private Color GetBiomeColor(float height, float latitude, float humidity, int x, int y)
    {
        if (height > snowHeight || latitude > (1 - polarRegionSize))
            return Color.white; // Snow color
        if (height > mountainHeight)
            return Color.gray; // Mountain color

        if (biomes == null || biomes.Length == 0 || voronoiPoints == null || voronoiPoints.Count == 0)
            return landColor;

        Vector2 pos = new Vector2(x, y);
        
        // Encontrar os dois pontos Voronoi mais próximos
        float minDist1 = float.MaxValue;
        float minDist2 = float.MaxValue;
        int closest1 = 0; // Inicializado com 0 em vez de -1
        int closest2 = 0; // Inicializado com 0 em vez de -1

        foreach (var point in voronoiPoints)
        {
            float dist = Vector2.Distance(pos, point.position);
            if (dist < minDist1)
            {
                minDist2 = minDist1;
                closest2 = closest1;
                minDist1 = dist;
                closest1 = point.biomeType;
            }
            else if (dist < minDist2)
            {
                minDist2 = dist;
                closest2 = point.biomeType;
            }
        }

        // Garantir que os índices estão dentro dos limites
        closest1 = Mathf.Clamp(closest1, 0, biomes.Length - 1);
        closest2 = Mathf.Clamp(closest2, 0, biomes.Length - 1);

        // Calcular borda com Simplex Noise
        float noiseValue = SimplexNoise(x * 0.1f, y * 0.1f);
        float edgeValue = (minDist2 - minDist1) / Mathf.Max(1f, cellSize);
        
        // Ajuste dinâmico do threshold baseado no cellSize
        float dynamicThreshold = voronoiThreshold * (32f / Mathf.Max(1f, cellSize));
        
        // Se estiver muito próximo da borda entre dois biomas, interpolar
        if (edgeValue < 0.1f)
        {
            return Color.Lerp(biomes[closest1].biomeColor, biomes[closest2].biomeColor, 0.5f);
        }

        return biomes[closest1].biomeColor;
    }

    private float SimplexNoise(float x, float y)
    {
        const float F2 = 0.366025404f; // (sqrt(3) - 1) / 2
        const float G2 = 0.211324865f; // (3 - sqrt(3)) / 6

        float n0, n1, n2;

        float s = (x + y) * F2;
        float xs = x + s;
        float ys = y + s;
        int i = FastFloor(xs);
        int j = FastFloor(ys);

        float t = (i + j) * G2;
        float X0 = i - t;
        float Y0 = j - t;
        float x0 = x - X0;
        float y0 = y - Y0;

        int i1, j1;
        if (x0 > y0)
        {
            i1 = 1;
            j1 = 0;
        }
        else
        {
            i1 = 0;
            j1 = 1;
        }

        float x1 = x0 - i1 + G2;
        float y1 = y0 - j1 + G2;
        float x2 = x0 - 1f + 2f * G2;
        float y2 = y0 - 1f + 2f * G2;

        int ii = i & 255;
        int jj = j & 255;

        // Calcular contribuições dos três cantos
        float t0 = 0.5f - x0 * x0 - y0 * y0;
        if (t0 < 0f) n0 = 0f;
        else
        {
            t0 *= t0;
            n0 = t0 * t0 * GradCoord(ii, jj, x0, y0);
        }

        float t1 = 0.5f - x1 * x1 - y1 * y1;
        if (t1 < 0f) n1 = 0f;
        else
        {
            t1 *= t1;
            n1 = t1 * t1 * GradCoord(ii + i1, jj + j1, x1, y1);
        }

        float t2 = 0.5f - x2 * x2 - y2 * y2;
        if (t2 < 0f) n2 = 0f;
        else
        {
            t2 *= t2;
            n2 = t2 * t2 * GradCoord(ii + 1, jj + 1, x2, y2);
        }

        return 40f * (n0 + n1 + n2);
    }

    private int FastFloor(float x)
    {
        return x > 0 ? (int)x : (int)x - 1;
    }

    private float GradCoord(int x, int y, float xd, float yd)
    {
        int hash = (x * 1619 + y * 31337) ^ seed;
        hash = hash * hash * hash * 60493;
        hash = (hash >> 13) ^ hash;

        float xg = (hash & 4) == 0 ? xd : -xd;
        float yg = (hash & 8) == 0 ? yd : -yd;

        return xg + yg;
    }

    void DrawNoiseMap()
    {
        try {
            if (noiseMap == null || landRenderer == null || waterRenderer == null)
            {
                Debug.LogError("[MapGenerator] Required components are null");
                return;
            }

            int textureWidth = Mathf.Max(1, width * pixelSize);
            int textureHeight = Mathf.Max(1, height * pixelSize);

            landTexture = new Texture2D(textureWidth, textureHeight);
            waterTexture = new Texture2D(textureWidth, textureHeight);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color terrainColor = GetTerrainColor(x, y);
                    Color waterCol = GetWaterColor(x, y);

                    FillTextureRegion(landTexture, x * pixelSize, y * pixelSize, pixelSize, terrainColor);
                    FillTextureRegion(waterTexture, x * pixelSize, y * pixelSize, pixelSize, waterCol);
                }
            }

            landTexture.Apply();
            waterTexture.Apply();

            if (landRenderer != null && waterRenderer != null)
            {
                landRenderer.sprite = Sprite.Create(landTexture, 
                    new Rect(0, 0, textureWidth, textureHeight), 
                    new Vector2(0.5f, 0.5f));
                waterRenderer.sprite = Sprite.Create(waterTexture, 
                    new Rect(0, 0, textureWidth, textureHeight), 
                    new Vector2(0.5f, 0.5f));

                // Gerar biomask apenas se o renderer estiver disponível
                if (biomeMaskRenderer != null)
                {
                    GenerateBiomeMask();
                }
            }

        } catch (System.Exception e) {
            Debug.LogError($"[MapGenerator] Error in DrawNoiseMap: {e.Message}");
        }
    }

    private void FillTextureRegion(Texture2D texture, int startX, int startY, int size, Color color)
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                texture.SetPixel(startX + i, startY + j, color);
            }
        }
    }

    private Color GetTerrainColor(int x, int y)
    {
        try {
            if (noiseMap == null || x < 0 || x >= width || y < 0 || y >= height)
                return Color.black;

            float heightValue = noiseMap[x, y];
            if (heightValue <= landProportion)
                return new Color(0, 0, 0, 0);

            return landColor; // Keep using landColor as fallback

        } catch (System.Exception e) {
            Debug.LogError($"[MapGenerator] Error in GetTerrainColor: {e.Message}");
            return Color.black;
        }
    }

    private Color GetWaterColor(int x, int y)
    {
        try {
            if (noiseMap == null || x < 0 || x >= width || y < 0 || y >= height)
                return Color.black;

            float heightValue = noiseMap[x, y];
            if (heightValue > landProportion)
                return new Color(0, 0, 0, 0);

            // Prevent division by zero
            float oceanDepth = landProportion != 0 ? heightValue / landProportion : 0;

            if (oceanDepth < deepWaterThreshold)
                return deepOceanColor;
            if (oceanDepth < shallowWaterThreshold)
                return oceanColor;
            return shallowWaterColor;

        } catch (System.Exception e) {
            Debug.LogError($"[MapGenerator] Error in GetWaterColor: {e.Message}");
            return Color.black;
        }
    }
}