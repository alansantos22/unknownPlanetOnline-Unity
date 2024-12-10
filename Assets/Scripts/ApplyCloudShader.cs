
using UnityEngine;

public class ApplyCloudShader : MonoBehaviour
{
    public Texture2D noiseTexture; // Textura de ruído
    public Shader cloudShader; // O shader personalizado

    private Material cloudMaterial;

    void Start()
    {
        // Cria o material com o shader
        cloudMaterial = new Material(cloudShader);

        // Configura propriedades do material
        cloudMaterial.SetTexture("_MainTex", noiseTexture);
        cloudMaterial.SetFloat("_CloudScale", 1.0f);
        cloudMaterial.SetVector("_CloudSpeed", new Vector4(0.1f, 0.0f, 0.0f, 0.0f));
        cloudMaterial.SetFloat("_CloudIntensity", 1.0f);

        // Aplica o material ao Mesh Renderer
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = cloudMaterial;
        }
        else
        {
            Debug.LogWarning("Este objeto não possui um Mesh Renderer!");
        }
    }
}