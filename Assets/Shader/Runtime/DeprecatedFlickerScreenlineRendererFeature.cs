/*
using UnityEngine;
using UnityEngine.Rendering.Universal;


[System.Serializable]
public class FlickerScreenlineSettings
{
    [Range(0,1f)] public float noiseStrength;
    [Range(0,100f)] public float noiseAmount;
    [Range(0,1f)] public float noiseIntensity;
    [Range(0,1f)] public float scanlineStrength;
    [Range(0,1000f)] public float scanlineAmount;
    
}

public class FlickerScreenlineRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private FlickerScreenlineSettings settings;
    [SerializeField] private Shader shader;
    [SerializeField] private Material material;
    private FlickerScreenlineRenderPass flickerScreenlineRenderPass;
    private Material tempMaterial;



    public override void Create()
    {
        if (shader == null || material == null)
        {
            return;
        }
        tempMaterial = new Material(material);
        flickerScreenlineRenderPass = new FlickerScreenlineRenderPass(tempMaterial, settings);

        flickerScreenlineRenderPass.renderPassEvent = RenderPassEvent.Before;
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (flickerScreenlineRenderPass == null)
        { 
            return;
        }                
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(flickerScreenlineRenderPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (Application.isPlaying)
        {
            Destroy(tempMaterial);
        }
        else
        {
            DestroyImmediate(tempMaterial);
        }
    }

}
*/