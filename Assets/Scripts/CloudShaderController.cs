
using UnityEngine;

public class CloudShaderController : MonoBehaviour
{
    public Material cloudMaterial;

    void Update()
    {
        if (cloudMaterial != null)
        {
            cloudMaterial.SetFloat("_Time", Time.time);
        }
    }
}