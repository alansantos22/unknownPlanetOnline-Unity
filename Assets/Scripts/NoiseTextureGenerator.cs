
using UnityEngine;

public class NoiseTextureGenerator : MonoBehaviour
{
    public int textureWidth = 256;
    public int textureHeight = 256;
    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;

    public Texture2D GenerateNoiseTexture()
    {
        Texture2D noiseTexture = new Texture2D(textureWidth, textureHeight);

        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                float xCoord = (float)x / textureWidth * scale + offsetX;
                float yCoord = (float)y / textureHeight * scale + offsetY;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                Color color = new Color(sample, sample, sample);
                noiseTexture.SetPixel(x, y, color);
            }
        }

        noiseTexture.Apply();
        return noiseTexture;
    }

    void Start()
    {
        Texture2D noiseTexture = GenerateNoiseTexture();
        SaveTextureAsPNG(noiseTexture, "Assets/NoiseTexture.png");
    }

    void SaveTextureAsPNG(Texture2D texture, string path)
    {
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log("Texture saved to " + path);
    }
}