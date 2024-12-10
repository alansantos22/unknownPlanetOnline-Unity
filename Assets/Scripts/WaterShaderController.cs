
using UnityEngine;

public class WaterShaderController : MonoBehaviour
{
    public Material waterMaterial;

    void Update()
    {
        if (waterMaterial != null)
        {
            waterMaterial.SetFloat("_Time", Time.time);
        }
    }
}