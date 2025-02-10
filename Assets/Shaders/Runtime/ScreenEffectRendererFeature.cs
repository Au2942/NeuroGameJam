using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

[System.Serializable]
public class ScreenEffectsSettings
{
    [Range(0,1f)] public float noiseStrength;
    [Range(0,100f)] public float noiseAmount;
    [Range(0,1f)] public float noiseIntensity;
    [Range(0,1f)] public float scanlineStrength;
    [Range(0,1000f)] public int scanlineAmount;
    
    public ScreenEffectsSettings(float noiseStrength, float noiseAmount, float noiseIntensity, float scanlineStrength, int scanlineAmount)
    {
        this.noiseStrength = noiseStrength;
        this.noiseAmount = noiseAmount;
        this.noiseIntensity = noiseIntensity;
        this.scanlineStrength = scanlineStrength;
        this.scanlineAmount = scanlineAmount;
    }
}
public class ScreenEffectRendererFeature : ScriptableRendererFeature
{
    class ScreenEffectPass : ScriptableRenderPass
    {
        private ScreenEffectsSettings defaultSettings;
        private static readonly int noiseStrengthID = Shader.PropertyToID("_NoiseStrength");
        private static readonly int noiseAmountID = Shader.PropertyToID("_NoiseAmount");
        private static readonly int noiseIntensityID = Shader.PropertyToID("_NoiseIntensity");
        private static readonly int scanlineStrengthID = Shader.PropertyToID("_ScanlineStrength");
        private static readonly int scanlineAmountID = Shader.PropertyToID("_ScanlineAmount");
        const string m_PassName = "ScreenEffectPass";
        Material m_BlitMaterial;

        public void Setup(Material blitMaterial, ScreenEffectsSettings settings)
        {
            m_BlitMaterial = blitMaterial;
            defaultSettings = settings;
            requiresIntermediateTexture = true;
        }

        

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var stack = VolumeManager.instance.stack;
            var customEffect = stack.GetComponent<ScreenEffectVolumeComponent>();

            if(!customEffect.IsActive())
            {
                return;
            }

            ScreenEffectsSettings volumeSettings = new ScreenEffectsSettings(customEffect.noiseStrength.value, customEffect.noiseAmount.value, customEffect.noiseIntensity.value, customEffect.scanlineStrength.value, customEffect.scanlineAmount.value);
           

            var resourceData = frameData.Get<UniversalResourceData>();

            if(resourceData.isActiveTargetBackBuffer)
            {
                Debug.LogError("ScreenEffectRendererFeature requires intermediate texture. Can't use back buffer as a texture input.");
                return;
            }
            var source = resourceData.activeColorTexture;
            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = $"CameraColor-{m_PassName}";
            destinationDesc.clearBuffer = false;

            UpdateScreenEffectSettings(volumeSettings);
            
            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);
            RenderGraphUtils.BlitMaterialParameters para = new(source, destination, m_BlitMaterial, 0);
            renderGraph.AddBlitPass(para, m_PassName);
            
            resourceData.cameraColor = destination;
        }

        private void UpdateScreenEffectSettings(ScreenEffectsSettings settings = null)
        {
            if (m_BlitMaterial == null) return;

            if(settings != null)
            {
                m_BlitMaterial.SetFloat(noiseStrengthID, settings.noiseStrength);
                m_BlitMaterial.SetFloat(noiseAmountID, settings.noiseAmount);
                m_BlitMaterial.SetFloat(noiseIntensityID, settings.noiseIntensity);
                m_BlitMaterial.SetFloat(scanlineStrengthID, settings.scanlineStrength);
                m_BlitMaterial.SetInteger(scanlineAmountID, settings.scanlineAmount);
            }
            else
            {
                m_BlitMaterial.SetFloat(noiseStrengthID, defaultSettings.noiseStrength);
                m_BlitMaterial.SetFloat(noiseAmountID, defaultSettings.noiseAmount);
                m_BlitMaterial.SetFloat(noiseIntensityID, defaultSettings.noiseIntensity);
                m_BlitMaterial.SetFloat(scanlineStrengthID, defaultSettings.scanlineStrength);
                m_BlitMaterial.SetInteger(scanlineAmountID, defaultSettings.scanlineAmount);
            }

        }

    }
    
    public RenderPassEvent injectionPoint = RenderPassEvent.BeforeRenderingPostProcessing;
    public ScreenEffectsSettings settings;
    public Material material;
    ScreenEffectPass m_ScriptablePass;
    Material tempMaterial;

    /// <inheritdoc/>
    public override void Create()
    {
        tempMaterial = new Material(material);
        m_ScriptablePass = new ScreenEffectPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = injectionPoint;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if(renderingData.cameraData.camera != Camera.main)
            return;
        if(material == null)
        {
            Debug.LogError("ScreenEffectRendererFeature material is null and will be skipped.");
            return;
        }
        
        m_ScriptablePass.Setup(tempMaterial, settings);
        renderer.EnqueuePass(m_ScriptablePass);

    }

    protected override void Dispose(bool disposing)
    {
        if (Application.isPlaying)
        {
            Destroy(tempMaterial);
        }
        else
        {
            DestroyImmediate(tempMaterial, true);
        }
    }

}
