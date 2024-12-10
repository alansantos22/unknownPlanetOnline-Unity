using UnityEngine;

public class HexMapGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float hexSize = 1f;
    public GameObject hexPrefab;
    public MapGenerator mapGenerator;

    void Start()
    {
        if (mapGenerator == null)
        {
            Debug.LogError("MapGenerator reference is not set.");
            return;
        }

        if (hexPrefab == null)
        {
            Debug.LogError("HexPrefab reference is not set.");
            return;
        }

        if (mapGenerator.noiseMap == null)
        {
            Debug.LogError("NoiseMap is not generated.");
            return;
        }

        GenerateHexMap();
    }

    void GenerateHexMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xPos = x * hexSize * 0.75f;
                float yPos = y * hexSize * Mathf.Sqrt(3) / 2 + (x % 2 == 0 ? 0 : hexSize * Mathf.Sqrt(3) / 4);

                Vector3 position = new Vector3(xPos, yPos, -2); // Adjusted Z position to -2
                GameObject hex = Instantiate(hexPrefab, position, Quaternion.identity, transform);

                float noiseValue = mapGenerator.noiseMap[x, y];
                hex.GetComponent<Hex>().Initialize(noiseValue);
            }
        }
    }
}